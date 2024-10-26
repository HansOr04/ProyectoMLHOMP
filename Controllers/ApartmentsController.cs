using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoMLHOMP.Controllers
{
    [Authorize]
    public class ApartmentsController : Controller
    {
        private readonly ProyectoContext _context;
        private readonly ILogger<ApartmentsController> _logger;

        public ApartmentsController(ProyectoContext context, ILogger<ApartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado o ID de usuario inválido");
            }
            return userId;
        }

        // GET: Apartments
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userIsHost = User.IsInRole("Host");

                ViewData["CurrentUserId"] = userId;
                ViewData["IsHost"] = userIsHost;

                var apartments = userIsHost
                    ? await _context.Apartment
                        .Include(a => a.Owner)
                        .Where(a => a.UserId == userId)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync()
                    : await _context.Apartment
                        .Include(a => a.Owner)
                        .Where(a => a.IsAvailable)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToListAsync();

                if (userIsHost && !apartments.Any())
                {
                    TempData["Info"] = "No tienes departamentos registrados. ¡Comienza a publicar ahora!";
                }

                return View(apartments);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los apartamentos");
                TempData["Error"] = "Ocurrió un error al cargar los departamentos";
                return View(new List<Apartment>());
            }
        }

        // GET: Apartments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID de departamento no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var apartment = await _context.Apartment
                    .Include(a => a.Owner)
                    .Include(a => a.Bookings)
                    .Include(a => a.Reviews)
                        .ThenInclude(r => r.Reviewer)
                    .FirstOrDefaultAsync(m => m.ApartmentId == id);

                if (apartment == null)
                {
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                ViewBag.IsOwner = apartment.UserId == userId;
                ViewBag.CurrentUserId = userId;

                return View(apartment);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar los detalles del departamento {id}");
                TempData["Error"] = "Error al cargar los detalles del departamento";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Apartments/Create
        [Authorize(Roles = "Host")]
        public IActionResult Create()
        {
            return View(new Apartment());
        }

        // POST: Apartments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Create([Bind("Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy")] Apartment apartment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    apartment.UserId = GetCurrentUserId();
                    apartment.CreatedAt = DateTime.UtcNow;
                    apartment.IsAvailable = true;

                    _context.Add(apartment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Departamento creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (UnauthorizedAccessException)
                {
                    return RedirectToAction("Login", "Users");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear apartamento");
                    ModelState.AddModelError("", "Ocurrió un error al crear el departamento");
                    TempData["Error"] = "Error al crear el departamento";
                }
            }
            return View(apartment);
        }

        // GET: Apartments/Edit/5
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID de departamento no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var apartment = await _context.Apartment.FindAsync(id);
                if (apartment == null)
                {
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                if (apartment.UserId != userId)
                {
                    _logger.LogWarning($"Usuario {userId} intentó editar un departamento que no le pertenece: {id}");
                    TempData["Error"] = "No tienes permiso para editar este departamento";
                    return RedirectToAction(nameof(Index));
                }

                return View(apartment);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar el formulario de edición para el departamento {id}");
                TempData["Error"] = "Error al cargar el formulario de edición";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Apartments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Edit(int id, [Bind("ApartmentId,Title,Description,PricePerNight,Address,City,Country,Bedrooms,Bathrooms,MaxOccupancy,IsAvailable")] Apartment apartment)
        {
            if (id != apartment.ApartmentId)
            {
                TempData["Error"] = "ID de departamento no coincide";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = GetCurrentUserId();
                var originalApartment = await _context.Apartment.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ApartmentId == id);

                if (originalApartment == null)
                {
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                if (originalApartment.UserId != userId)
                {
                    _logger.LogWarning($"Usuario {userId} intentó editar un departamento que no le pertenece: {id}");
                    TempData["Error"] = "No tienes permiso para editar este departamento";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    apartment.UserId = userId;
                    apartment.UpdatedAt = DateTime.UtcNow;
                    apartment.CreatedAt = originalApartment.CreatedAt;

                    _context.Update(apartment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Departamento actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ApartmentExists(apartment.ApartmentId))
                {
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                _logger.LogError(ex, $"Error de concurrencia al actualizar el departamento {id}");
                TempData["Error"] = "Error al actualizar el departamento. Por favor, intente nuevamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar el departamento {id}");
                TempData["Error"] = "Error al actualizar el departamento";
            }

            return View(apartment);
        }

        // GET: Apartments/Delete/5
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID de departamento no especificado";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var apartment = await _context.Apartment
                    .Include(a => a.Owner)
                    .Include(a => a.Bookings)
                    .FirstOrDefaultAsync(m => m.ApartmentId == id);

                if (apartment == null)
                {
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var userId = GetCurrentUserId();
                if (apartment.UserId != userId)
                {
                    _logger.LogWarning($"Usuario {userId} intentó eliminar un departamento que no le pertenece: {id}");
                    TempData["Error"] = "No tienes permiso para eliminar este departamento";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si hay reservas futuras
                if (apartment.Bookings?.Any(b => b.StartDate > DateTime.Today) ?? false)
                {
                    TempData["Error"] = "No se puede eliminar el departamento porque tiene reservas futuras";
                    return RedirectToAction(nameof(Index));
                }

                return View(apartment);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar la vista de eliminación para el departamento {id}");
                TempData["Error"] = "Error al cargar la vista de eliminación";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Apartments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var apartment = await _context.Apartment
                    .Include(a => a.Bookings)
                    .FirstOrDefaultAsync(a => a.ApartmentId == id);

                if (apartment == null)
                {
                    TempData["Error"] = "Departamento no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                if (apartment.UserId != userId)
                {
                    _logger.LogWarning($"Usuario {userId} intentó eliminar un departamento que no le pertenece: {id}");
                    TempData["Error"] = "No tienes permiso para eliminar este departamento";
                    return RedirectToAction(nameof(Index));
                }

                if (apartment.Bookings?.Any(b => b.StartDate > DateTime.Today) ?? false)
                {
                    TempData["Error"] = "No se puede eliminar el departamento porque tiene reservas futuras";
                    return RedirectToAction(nameof(Index));
                }

                _context.Apartment.Remove(apartment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Departamento eliminado exitosamente";
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar el departamento {id}");
                TempData["Error"] = "Error al eliminar el departamento";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ApartmentExists(int id)
        {
            return _context.Apartment.Any(e => e.ApartmentId == id);
        }

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