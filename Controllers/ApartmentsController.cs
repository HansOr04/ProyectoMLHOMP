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

        // Método auxiliar para obtener el ID del usuario de forma segura
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

                if (userIsHost)
                {
                    return View(await _context.Apartment
                        .Include(a => a.Owner)
                        .Where(a => a.UserId == userId)
                        .ToListAsync());
                }
                else
                {
                    return View(await _context.Apartment
                        .Include(a => a.Owner)
                        .Where(a => a.IsAvailable)
                        .ToListAsync());
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Users");
            }
        }

        // GET: Apartments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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
                var userId = GetCurrentUserId();
                ViewBag.IsOwner = apartment.UserId == userId;
            }
            catch (UnauthorizedAccessException)
            {
                ViewBag.IsOwner = false;
            }

            return View(apartment);
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
                    return RedirectToAction(nameof(Index));
                }
                catch (UnauthorizedAccessException)
                {
                    return RedirectToAction("Login", "Users");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear apartamento");
                    ModelState.AddModelError("", "Ocurrió un error al crear el apartamento");
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

        // POST: Apartments/Edit/5
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

        // GET: Apartments/Delete/5
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

        // POST: Apartments/Delete/5
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