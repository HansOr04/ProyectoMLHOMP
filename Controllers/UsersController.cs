using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProyectoMLHOMP.Models;
using ProyectoMLHOMP.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;

namespace ProyectoMLHOMP.Controllers
{
    public class UsersController : Controller
    {
        private readonly DataProyecto _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UsersController> _logger;

        public UsersController(DataProyecto context, IWebHostEnvironment webHostEnvironment, ILogger<UsersController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("FirstName,LastName,DateOfBirth,Address,City,Country,Email,PhoneNumber,Username,PasswordHash,Biography,Languages")] User user, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation($"Attempting to register user: {user.Username}");

                    // Verificar si el email o username ya existen
                    if (await _context.User.AnyAsync(u => u.Email == user.Email || u.Username == user.Username))
                    {
                        ModelState.AddModelError("", "El email o nombre de usuario ya están en uso.");
                        return View(user);
                    }

                    // Hashear la contraseña
                    user.PasswordHash = HashPassword(user.PasswordHash);

                    // Establecer valores predeterminados
                    user.RegistrationDate = DateTime.UtcNow;
                    user.IsHost = false;
                    user.IsVerified = false;

                    // Manejar la carga de la imagen de perfil
                    if (ProfileImage != null && ProfileImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + ProfileImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProfileImage.CopyToAsync(fileStream);
                        }

                        user.ProfileImageUrl = "/images/profiles/" + uniqueFileName;
                    }
                    else
                    {
                        user.ProfileImageUrl = "/images/default-profile.jpg";
                    }

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"User registered successfully: {user.Username}");
                    return RedirectToAction("Index", "Apartments");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error registering user: {user.Username}");
                    ModelState.AddModelError("", "Ocurrió un error al registrar el usuario. Por favor, inténtalo de nuevo.");
                }
            }
            else
            {
                _logger.LogWarning("Invalid model state during user registration");
            }

            return View(user);
        }

        


// GET: Users/Login
public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid) // Verifica que el formulario sea válido
            {
                try
                {
                    // Busca al usuario por correo electrónico
                    var user = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (user != null && VerifyPassword(model.Password, user.PasswordHash)) // Verifica la contraseña
                    {
                        // Crear los claims (información del usuario para la autenticación)
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                };

                        // Crear la identidad del usuario con los claims
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        // Iniciar sesión con cookies
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        _logger.LogInformation($"User logged in successfully: {user.Username}");

                        // Redirigir al usuario a la página principal
                        return RedirectToAction("Index", "Apartments");
                    }

                    // Si la contraseña o el usuario no son correctos
                    _logger.LogWarning($"Failed login attempt for email: {model.Email}");
                    ModelState.AddModelError("", "Correo electrónico o contraseña incorrectos.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during login process for email: {model.Email}");
                    ModelState.AddModelError("", "Ocurrió un error durante el inicio de sesión. Por favor, inténtalo de nuevo.");
                }
            }

            return View(model); // Si algo falla, devuelve la vista con el modelo
        }

        // GET: Users/Logout
        public async Task<IActionResult> Logout()
        {
            // Eliminar la cookie de autenticación
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User logged out successfully.");

            // Redirigir al usuario a la página de inicio o página de login
            return RedirectToAction("Index", "Home");
        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,FirstName,LastName,DateOfBirth,Address,City,Country,Email,PhoneNumber,Username,Biography,Languages")] User user, IFormFile ProfileImage)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.User.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Manejar la carga de la nueva imagen de perfil si se proporciona
                    if (ProfileImage != null && ProfileImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + ProfileImage.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProfileImage.CopyToAsync(fileStream);
                        }

                        // Eliminar la imagen anterior si existe
                        if (!string.IsNullOrEmpty(existingUser.ProfileImageUrl) && existingUser.ProfileImageUrl != "/images/default-profile.jpg")
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingUser.ProfileImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        user.ProfileImageUrl = "/images/profiles/" + uniqueFileName;
                    }
                    else
                    {
                        // Mantener la URL de la imagen existente si no se carga una nueva
                        user.ProfileImageUrl = existingUser.ProfileImageUrl;
                    }

                    // Mantener los valores que no deben cambiar
                    user.PasswordHash = existingUser.PasswordHash;
                    user.RegistrationDate = existingUser.RegistrationDate;
                    user.IsHost = existingUser.IsHost;
                    user.IsVerified = existingUser.IsVerified;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"User updated successfully: {user.Username}");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, $"Concurrency error while updating user: {user.Username}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating user: {user.Username}");
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el usuario. Por favor, inténtalo de nuevo.");
                    return View(user);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                try
                {
                    // Eliminar la imagen de perfil si existe
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl) && user.ProfileImageUrl != "/images/default-profile.jpg")
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    _context.User.Remove(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"User deleted successfully: {user.Username}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting user: {user.Username}");
                    // Puedes manejar el error aquí, por ejemplo, mostrando un mensaje al usuario
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}