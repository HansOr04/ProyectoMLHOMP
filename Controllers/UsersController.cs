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

        // GET: Users/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var user = await _context.User
                    .Include(u => u.ApartmentsOwned)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    _logger.LogWarning($"Usuario {userId} no encontrado al intentar ver el perfil");
                    return RedirectToAction("Index", "Home");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el perfil del usuario");
                TempData["Error"] = "Error al cargar el perfil";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Users/Edit
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var user = await _context.User.FindAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning($"Usuario {userId} no encontrado al intentar editar");
                    return RedirectToAction("Index", "Home");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el formulario de edición del perfil");
                TempData["Error"] = "Error al cargar el formulario de edición";
                return RedirectToAction("Profile");
            }
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
                    }
                }

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
                    TempData["Success"] = "¡Registro exitoso! Bienvenido a nuestra plataforma.";
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

        // POST: Users/Edit
        // POST: Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit([Bind("FirstName,LastName,Address,City,Country,PhoneNumber,Biography,Languages,IsHost,ProfileImageUrl")] User updatedUser, IFormFile? profileImage)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var user = await _context.User.FindAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning($"Usuario {userId} no encontrado al intentar actualizar");
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index", "Home");
                }

                // Limpiar ModelState de los campos que no queremos validar
                ModelState.Remove("Email");
                ModelState.Remove("Username");
                ModelState.Remove("Password");
                ModelState.Remove("PasswordHash");
                ModelState.Remove("DateOfBirth");

                if (!ModelState.IsValid)
                {
                    // Mantener los datos originales para los campos no editables
                    updatedUser.Email = user.Email;
                    updatedUser.Username = user.Username;
                    updatedUser.DateOfBirth = user.DateOfBirth;
                    return View(updatedUser);
                }

                // Procesar nueva imagen de perfil si se proporciona
                if (profileImage != null && profileImage.Length > 0)
                {
                    if (profileImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfileImage", "La imagen no puede superar los 5MB");
                        return View(updatedUser);
                    }

                    if (!IsValidImageFile(profileImage))
                    {
                        ModelState.AddModelError("ProfileImage", "El archivo debe ser una imagen válida (jpg, jpeg, png)");
                        return View(updatedUser);
                    }

                    var (success, imagePath, errorMessage) = await ProcessProfileImage(profileImage, user.ProfileImageUrl);
                    if (!success)
                    {
                        ModelState.AddModelError("ProfileImage", errorMessage);
                        return View(updatedUser);
                    }
                    user.ProfileImageUrl = imagePath;
                }

                // Actualizar solo los campos permitidos
                user.FirstName = updatedUser.FirstName?.Trim() ?? user.FirstName;
                user.LastName = updatedUser.LastName?.Trim() ?? user.LastName;
                user.Address = updatedUser.Address?.Trim() ?? user.Address;
                user.City = updatedUser.City?.Trim() ?? user.City;
                user.Country = updatedUser.Country?.Trim() ?? user.Country;
                user.PhoneNumber = updatedUser.PhoneNumber?.Trim() ?? user.PhoneNumber;
                user.Biography = updatedUser.Biography?.Trim() ?? user.Biography;
                user.Languages = updatedUser.Languages?.Trim() ?? user.Languages;

                // Verificar cambio en el estado de host
                bool isHostChanged = user.IsHost != updatedUser.IsHost;
                user.IsHost = updatedUser.IsHost;

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    // Si cambió el estado de host, actualizar los claims
                    if (isHostChanged)
                    {
                        await RefreshUserClaims(user);
                        TempData["Success"] = user.IsHost
                            ? "¡Perfil actualizado exitosamente! Bienvenido como anfitrión."
                            : "Perfil actualizado exitosamente. Modo anfitrión desactivado.";
                    }
                    else
                    {
                        TempData["Success"] = "Perfil actualizado exitosamente.";
                    }

                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, $"Error de concurrencia al actualizar el usuario {userId}");
                    TempData["Error"] = "Error al guardar los cambios. Por favor, intente nuevamente.";
                    return View(updatedUser);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil");
                TempData["Error"] = "Error al actualizar el perfil. Por favor, intente nuevamente.";
                return View(updatedUser);
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
                TempData["Success"] = $"¡Bienvenido de nuevo, {user.FirstName}!";

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
            TempData["Success"] = "Has cerrado sesión exitosamente.";
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

        private async Task RefreshUserClaims(User user)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await LoginUser(user);
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }

        private bool IsValidImageFile(IFormFile file)
        {
            // Lista de tipos MIME permitidos para imágenes
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
            return allowedTypes.Contains(file.ContentType.ToLower());
        }

        private async Task<(bool success, string imagePath, string errorMessage)> ProcessProfileImage(IFormFile profileImage, string currentImagePath)
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

                // Eliminar la imagen anterior si existe y no es la imagen por defecto
                if (currentImagePath != "/images/default-profile.jpg")
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, currentImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                return (true, $"/images/profiles/{uniqueFileName}", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la imagen del perfil");
                return (false, string.Empty, "Error al procesar la imagen del perfil.");
            }
        }
    }
}