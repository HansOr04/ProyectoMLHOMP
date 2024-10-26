using System; // Espacio de nombres para clases y tipos básicos del sistema
using System.Security.Claims; // Para trabajar con claims de seguridad, como el ID de usuario
using Microsoft.AspNetCore.Mvc; // Espacio de nombres para controladores y acciones en ASP.NET Core MVC
using Microsoft.EntityFrameworkCore; // Para interacciones con la base de datos mediante Entity Framework Core
using ProyectoMLHOMP.Models; // Modelos específicos del proyecto
using Microsoft.AspNetCore.Authorization; // Para manejo de autorizaciones en el controlador

namespace ProyectoMLHOMP.Controllers
{
    // Requiere que el usuario esté autenticado para acceder a las acciones del controlador
    [Authorize]
    public class ApartmentsController : Controller
    {
        // Contexto de base de datos para realizar operaciones CRUD
        private readonly ProyectoContext _context;
        // Logger para registrar información y errores
        private readonly ILogger<ApartmentsController> _logger;

        // Constructor del controlador que inyecta dependencias del contexto y logger
        public ApartmentsController(ProyectoContext context, ILogger<ApartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método privado para obtener el ID del usuario actual autenticado
        private int GetCurrentUserId()
        {
            // Obtiene el claim del ID de usuario desde el token de autenticación
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Verifica si el ID de usuario es válido
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // Lanza una excepción si el usuario no está autenticado o el ID es inválido
                throw new UnauthorizedAccessException("Usuario no autenticado o ID de usuario inválido");
            }
            return userId; // Retorna el ID de usuario
        }

        // Acción para mostrar la lista de departamentos
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtiene el ID del usuario actual
                var userId = GetCurrentUserId();
                // Verifica si el usuario es un anfitrión (Host)
                var userIsHost = User.IsInRole("Host");

                // Almacena el ID de usuario y el rol de anfitrión en la vista
                ViewData["CurrentUserId"] = userId;
                ViewData["IsHost"] = userIsHost;

                // Si es un anfitrión, muestra sus departamentos; de lo contrario, muestra los disponibles
                var apartments = userIsHost
                    ? await _context.Apartment
                        .Include(a => a.Owner) // Incluye detalles del propietario
                        .Where(a => a.UserId == userId)
                        .OrderByDescending(a => a.CreatedAt) // Ordena por fecha de creación
                        .ToListAsync()
                    : await _context.Apartment
                        .Include(a => a.Owner)
                        .Where(a => a.IsAvailable)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync();

                // Mensaje informativo si el anfitrión no tiene departamentos registrados
                if (userIsHost && !apartments.Any())
                {
                    TempData["Info"] = "No tienes departamentos registrados. ¡Comienza a publicar ahora!";
                }

                // Devuelve la vista con la lista de departamentos
                return View(apartments);
            }
            catch (UnauthorizedAccessException)
            {
                // Redirige a la página de inicio de sesión si el usuario no está autenticado
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                // Registra errores y muestra un mensaje al usuario
                _logger.LogError(ex, "Error al cargar los apartamentos");
                TempData["Error"] = "Ocurrió un error al cargar los departamentos";
                return View(new List<Apartment>()); // Devuelve una lista vacía si hay error
            }
        }

        // Acción para mostrar los detalles de un departamento específico
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                // Verifica si el ID no fue especificado y redirige a la lista de departamentos
                TempData["Error"] = "ID de departamento no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Busca el departamento por su ID e incluye detalles adicionales
                var apartment = await _context.Apartment
                    .Include(a => a.Owner)
                    .Include(a => a.Bookings)
                    .Include(a => a.Reviews)
                        .ThenInclude(r => r.Reviewer)
                    .FirstOrDefaultAsync(m => m.ApartmentId == id);

                if (apartment == null)
                {
                    // Si no encuentra el departamento, muestra un mensaje y redirige a la lista
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verifica si el usuario actual es el propietario del departamento
                var userId = GetCurrentUserId();
                ViewBag.IsOwner = apartment.UserId == userId;
                ViewBag.CurrentUserId = userId;

                // Devuelve la vista con los detalles del departamento
                return View(apartment);
            }
            catch (UnauthorizedAccessException)
            {
                // Redirige a la página de inicio de sesión si el usuario no está autenticado
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                // Registra errores y muestra un mensaje al usuario
                _logger.LogError(ex, $"Error al cargar los detalles del departamento {id}");
                TempData["Error"] = "Error al cargar los detalles del departamento";
                return RedirectToAction(nameof(Index));
            }
        }

        // Acción para mostrar el formulario de creación de departamentos, solo accesible para anfitriones
        [Authorize(Roles = "Host")]
        public IActionResult Create()
        {
            // Devuelve la vista del formulario vacío para crear un nuevo departamento
            return View(new Apartment());
        }

        // Acción POST para crear un nuevo departamento
        [HttpPost]
        [ValidateAntiForgeryToken] // Previene ataques de tipo Cross-Site Request Forgery
        [Authorize(Roles = "Host")] // Solo los anfitriones pueden crear departamentos
        public async Task<IActionResult> Create([Bind("Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy")] Apartment apartment)
        {
            if (ModelState.IsValid) // Verifica si el modelo es válido
            {
                try
                {
                    // Asigna propiedades adicionales al departamento
                    apartment.UserId = GetCurrentUserId();
                    apartment.CreatedAt = DateTime.UtcNow;
                    apartment.IsAvailable = true;

                    // Agrega el departamento a la base de datos y guarda cambios
                    _context.Add(apartment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Departamento creado exitosamente";
                    return RedirectToAction(nameof(Index)); // Redirige a la lista de departamentos
                }
                catch (UnauthorizedAccessException)
                {
                    // Redirige a la página de inicio de sesión si el usuario no está autenticado
                    return RedirectToAction("Login", "Users");
                }
                catch (Exception ex)
                {
                    // Registra errores y muestra un mensaje al usuario
                    _logger.LogError(ex, "Error al crear apartamento");
                    ModelState.AddModelError("", "Ocurrió un error al crear el departamento");
                    TempData["Error"] = "Error al crear el departamento";
                }
            }
            // Devuelve la vista con el formulario y errores si los hay
            return View(apartment);
        }

        // Acción para editar un departamento, solo accesible para anfitriones
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                // Verifica si el ID no fue especificado y redirige a la lista de departamentos
                TempData["Error"] = "ID de departamento no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Busca el departamento por su ID
                var apartment = await _context.Apartment.FindAsync(id);
                if (apartment == null)
                {
                    // Si no encuentra el departamento, muestra un mensaje y redirige a la lista
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                if (apartment.UserId != userId)
                {
                    // Registra un intento no autorizado y redirige a la lista
                    _logger.LogWarning($"Usuario {userId} intentó editar un departamento que no le pertenece: {id}");
                    TempData["Error"] = "No tienes permiso para editar este departamento";
                    return RedirectToAction(nameof(Index));
                }

                // Devuelve la vista con el formulario de edición
                return View(apartment);
            }
            catch (UnauthorizedAccessException)
            {
                // Redirige a la página de inicio de sesión si el usuario no está autenticado
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                // Registra errores y muestra un mensaje al usuario
                _logger.LogError(ex, $"Error al cargar el formulario de edición para el departamento {id}");
                TempData["Error"] = "Error al cargar el formulario de edición";
                return RedirectToAction(nameof(Index));
            }
        }

        // Acción POST para actualizar el departamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Edit(int id, [Bind("ApartmentId,Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy,IsAvailable")] Apartment apartment)
        {
            if (id != apartment.ApartmentId)
            {
                // Si los IDs no coinciden, redirige con un mensaje de error
                TempData["Error"] = "Error de ID de departamento";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    if (apartment.UserId != userId)
                    {
                        // Registra un intento no autorizado y redirige a la lista
                        _logger.LogWarning($"Usuario {userId} intentó editar un departamento que no le pertenece: {id}");
                        TempData["Error"] = "No tienes permiso para editar este departamento";
                        return RedirectToAction(nameof(Index));
                    }

                    // Marca el departamento como modificado y guarda cambios
                    _context.Update(apartment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Departamento actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (UnauthorizedAccessException)
                {
                    // Redirige a la página de inicio de sesión si el usuario no está autenticado
                    return RedirectToAction("Login", "Users");
                }
                catch (Exception ex)
                {
                    // Registra errores y muestra un mensaje al usuario
                    _logger.LogError(ex, $"Error al actualizar el departamento {id}");
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el departamento");
                    TempData["Error"] = "Error al actualizar el departamento";
                }
            }

            // Devuelve la vista con el formulario y errores si los hay
            return View(apartment);
        }

        // Acción para eliminar un departamento, solo accesible para anfitriones
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                // Verifica si el ID no fue especificado y redirige a la lista de departamentos
                TempData["Error"] = "ID de departamento no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Busca el departamento por su ID
                var apartment = await _context.Apartment.FindAsync(id);
                if (apartment == null)
                {
                    // Si no encuentra el departamento, muestra un mensaje y redirige a la lista
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                if (apartment.UserId != userId)
                {
                    // Registra un intento no autorizado y redirige a la lista
                    _logger.LogWarning($"Usuario {userId} intentó eliminar un departamento que no le pertenece: {id}");
                    TempData["Error"] = "No tienes permiso para eliminar este departamento";
                    return RedirectToAction(nameof(Index));
                }

                // Devuelve la vista de confirmación para eliminar
                return View(apartment);
            }
            catch (UnauthorizedAccessException)
            {
                // Redirige a la página de inicio de sesión si el usuario no está autenticado
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                // Registra errores y muestra un mensaje al usuario
                _logger.LogError(ex, $"Error al cargar el formulario de eliminación para el departamento {id}");
                TempData["Error"] = "Error al cargar el formulario de eliminación";
                return RedirectToAction(nameof(Index));
            }
        }

        // Acción POST para confirmar la eliminación del departamento
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var apartment = await _context.Apartment.FindAsync(id);
                if (apartment == null)
                {
                    // Si no encuentra el departamento, muestra un mensaje y redirige a la lista
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                if (apartment.UserId != userId)
                {
                    // Registra un intento no autorizado y redirige a la lista
                    _logger.LogWarning($"Usuario {userId} intentó eliminar un departamento que no le pertenece: {id}");
                    TempData["Error"] = "No tienes permiso para eliminar este departamento";
                    return RedirectToAction(nameof(Index));
                }

                // Elimina el departamento de la base de datos y guarda cambios
                _context.Apartment.Remove(apartment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Departamento eliminado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                // Redirige a la página de inicio de sesión si el usuario no está autenticado
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                // Registra errores y muestra un mensaje al usuario
                _logger.LogError(ex, $"Error al eliminar el departamento {id}");
                TempData["Error"] = "Error al eliminar el departamento";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
