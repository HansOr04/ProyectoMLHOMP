using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoMLHOMP.Controllers
{
    // Requiere que el usuario esté autenticado para acceder a cualquier acción del controlador
    [Authorize]
    public class ApartmentsController : Controller
    {
        // Variables privadas para el contexto de la base de datos y el logger
        private readonly ProyectoContext _context;
        private readonly ILogger<ApartmentsController> _logger;

        // Constructor para inicializar el contexto y el logger
        public ApartmentsController(ProyectoContext context, ILogger<ApartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método auxiliar para obtener el ID del usuario autenticado de forma segura
        private int GetCurrentUserId()
        {
            // Obtener el ID del usuario desde los claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // Lanzar excepción si el ID no es válido o el usuario no está autenticado
                throw new UnauthorizedAccessException("Usuario no autenticado o ID de usuario inválido");
            }
            return userId;
        }

        // Acción para mostrar la lista de apartamentos (GET: Apartments)
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener el ID del usuario actual y verificar si es un "Host"
                var userId = GetCurrentUserId();
                var userIsHost = User.IsInRole("Host");

                // Pasar datos al ViewData para que estén disponibles en la vista
                ViewData["CurrentUserId"] = userId;
                ViewData["IsHost"] = userIsHost;

                if (userIsHost)
                {
                    // Si es un "Host", mostrar solo los apartamentos del usuario actual
                    return View(await _context.Apartment
                        .Include(a => a.Owner)
                        .Where(a => a.UserId == userId)
                        .ToListAsync());
                }
                else
                {
                    // Si no es un "Host", mostrar solo los apartamentos disponibles
                    return View(await _context.Apartment
                        .Include(a => a.Owner)
                        .Where(a => a.IsAvailable)
                        .ToListAsync());
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Redirigir al login si ocurre un problema de autenticación
                return RedirectToAction("Login", "Users");
            }
        }

        // Acción para mostrar los detalles de un apartamento específico (GET: Apartments/Details/5)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Buscar el apartamento por ID e incluir detalles del dueño, reservas y reseñas
            var apartment = await _context.Apartment
                .Include(a => a.Owner)
                .Include(a => a.Bookings)
                .Include(a => a.Reviews)
                .FirstOrDefaultAsync(m => m.ApartmentId == id);

            if (apartment == null)
            {
                return NotFound();
            }

            try
            {
                // Verificar si el usuario actual es el dueño del apartamento
                var userId = GetCurrentUserId();
                ViewBag.IsOwner = apartment.UserId == userId;
            }
            catch (UnauthorizedAccessException)
            {
                ViewBag.IsOwner = false;
            }

            return View(apartment);
        }

        // Acción para mostrar el formulario de creación de un nuevo apartamento (GET: Apartments/Create)
        [Authorize(Roles = "Host")]
        public IActionResult Create()
        {
            return View(new Apartment());
        }

        // Acción para crear un nuevo apartamento (POST: Apartments/Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Create([Bind("Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy")] Apartment apartment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Asignar propiedades adicionales antes de guardar
                    apartment.UserId = GetCurrentUserId();
                    apartment.CreatedAt = DateTime.UtcNow;
                    apartment.IsAvailable = true;

                    _context.Add(apartment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (UnauthorizedAccessException)
                {
                    return RedirectToAction("Login", "Users");
                }
                catch (Exception ex)
                {
                    // Manejo de errores y logeo
                    _logger.LogError(ex, "Error al crear apartamento");
                    ModelState.AddModelError("", "Ocurrió un error al crear el apartamento");
                }
            }
            return View(apartment);
        }

        // Acción para mostrar el formulario de edición de un apartamento existente (GET: Apartments/Edit/5)
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartment = await _context.Apartment.FindAsync(id);
            if (apartment == null)
            {
                return NotFound();
            }

            try
            {
                var userId = GetCurrentUserId();
                // Verificar que el usuario sea el dueño del apartamento antes de permitir la edición
                if (apartment.UserId != userId)
                {
                    return Forbid();
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }

            return View(apartment);
        }

        // Acción para editar un apartamento existente (POST: Apartments/Edit/5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Edit(int id, [Bind("ApartmentId,Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy,IsAvailable")] Apartment apartment)
        {
            if (id != apartment.ApartmentId)
            {
                return NotFound();
            }

            try
            {
                var userId = GetCurrentUserId();
                var originalApartment = await _context.Apartment.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ApartmentId == id);

                if (originalApartment?.UserId != userId)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    // Asignar datos antes de actualizar el registro
                    apartment.UserId = userId;
                    apartment.UpdatedAt = DateTime.UtcNow;
                    apartment.CreatedAt = originalApartment.CreatedAt;

                    _context.Update(apartment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApartmentExists(apartment.ApartmentId))
                {
                    return NotFound();
                }
                throw;
            }

            return View(apartment);
        }

        // Acción para mostrar la confirmación de eliminación de un apartamento (GET: Apartments/Delete/5)
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apartment = await _context.Apartment
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(m => m.ApartmentId == id);

            if (apartment == null)
            {
                return NotFound();
            }

            try
            {
                var userId = GetCurrentUserId();
                if (apartment.UserId != userId)
                {
                    return Forbid();
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }

            return View(apartment);
        }

        // Acción para eliminar un apartamento (POST: Apartments/Delete/5)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var apartment = await _context.Apartment.FindAsync(id);

                if (apartment == null)
                {
                    return NotFound();
                }

                if (apartment.UserId != userId)
                {
                    return Forbid();
                }

                _context.Apartment.Remove(apartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
        }

        // Método auxiliar para verificar si existe un apartamento con el ID dado
        private bool ApartmentExists(int id)
        {
            return _context.Apartment.Any(e => e.ApartmentId == id);
        }

        // Método auxiliar para comprobar si el usuario actual es el dueño de un apartamento
        private async Task<bool> IsApartmentOwner(int apartmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                return await _context.Apartment
                    .AnyAsync(a => a.ApartmentId == apartmentId && a.UserId == userId);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
