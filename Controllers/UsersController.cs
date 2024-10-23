using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoMLHOMP.Controllers
{
    public class UsersController : Controller
    {
        private readonly ProyectoContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ProyectoContext context, IWebHostEnvironment webHostEnvironment, ILogger<UsersController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Apartments");
            }
            return View(new User());
        }

        // POST: Users/Register
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(User user, IFormFile? profileImage)
{
    try
    {
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning($"Error de validación: {error.ErrorMessage}");
            }
            return View(user);
        }

        // Inicializar la tabla si es necesario
        if (!await _context.Database.CanConnectAsync())
        {
            _logger.LogInformation("Inicializando base de datos...");
            await _context.Database.EnsureCreatedAsync();
        }

        // Verificaciones de unicidad sin usar Any() inicialmente
        var existingUser = await _context.User
            .Where(u => u.Email == user.Email || u.Username == user.Username)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            if (existingUser.Email == user.Email)
            {
                ModelState.AddModelError("Email", "Este email ya está registrado");
            }
            if (existingUser.Username == user.Username)
            {
                ModelState.AddModelError("Username", "Este nombre de usuario ya está en uso");
            }
            return View(user);
        }

        // Procesar imagen
        string profileImagePath = "/images/default-profile.jpg";
        if (profileImage != null && profileImage.Length > 0)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }
                profileImagePath = $"/images/profiles/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al procesar la imagen: {ex.Message}");
                // Continuar con la imagen por defecto
            }
        }

        // Crear nuevo usuario
        var newUser = new User
        {
            FirstName = user.FirstName.Trim(),
            LastName = user.LastName.Trim(),
            DateOfBirth = user.DateOfBirth,
            Address = user.Address.Trim(),
            City = user.City.Trim(),
            Country = user.Country.Trim(),
            Email = user.Email.Trim().ToLower(),
            PhoneNumber = user.PhoneNumber.Trim(),
            Username = user.Username.Trim(),
            PasswordHash = Models.User.HashPassword(user.Password),
            Biography = user.Biography?.Trim() ?? "",
            Languages = user.Languages?.Trim() ?? "",
            ProfileImageUrl = profileImagePath,
            RegistrationDate = DateTime.UtcNow,
            IsHost = false,
            IsVerified = false
        };

        try
        {
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Usuario registrado exitosamente: {newUser.Email}");

            await LoginUser(newUser);
            return RedirectToAction("Index", "Apartments");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"Error al guardar en la base de datos: {ex.Message}");
            ModelState.AddModelError("", "Error al guardar el usuario. Por favor, intente nuevamente.");
            return View(user);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error general en el registro: {ex.Message}");
        ModelState.AddModelError("", "Ocurrió un error durante el registro. Por favor, intente nuevamente.");
        return View(user);
    }
}

        // GET: Users/Login
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            try
            {
                ViewData["ReturnUrl"] = returnUrl;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError(string.Empty, "Usuario y contraseña son requeridos");
                    return View();
                }

                var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado");
                    return View();
                }

                if (!user.VerifyPassword(password))
                {
                    ModelState.AddModelError(string.Empty, "Contraseña incorrecta");
                    return View();
                }

                await LoginUser(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el login");
                ModelState.AddModelError(string.Empty, "Error al iniciar sesión. Por favor, intente nuevamente.");
                return View();
            }
        }

        // POST: Users/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task LoginUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.GetFullName()),
                new Claim(ClaimTypes.Role, user.IsHost ? "Host" : "Guest")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}