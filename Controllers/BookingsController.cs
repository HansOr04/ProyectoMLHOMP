using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ProyectoMLHOMP.Models;

namespace ProyectoMLHOMP.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ProyectoContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ProyectoContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Bookings
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var isHost = User.IsInRole("Host");

            if (isHost)
            {
                // Si es host, mostrar las reservas de sus apartamentos
                var hostBookings = await _context.Booking
                    .Include(b => b.Apartment)
                    .Include(b => b.Guest)
                    .Where(b => b.Apartment.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
                return View(hostBookings);
            }
            else
            {
                // Si es guest, mostrar sus reservas
                var userBookings = await _context.Booking
                    .Include(b => b.Apartment)
                    .Include(b => b.Guest)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
                return View(userBookings);
            }
        }

        // GET: Bookings/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Apartment)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (booking.UserId != userId && booking.Apartment.UserId != userId)
            {
                return Forbid();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        [Authorize]
        public async Task<IActionResult> Create(int apartmentId)
        {
            var apartment = await _context.Apartment
                .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId);

            if (apartment == null)
            {
                return NotFound();
            }

            if (!apartment.IsAvailable)
            {
                TempData["Error"] = "Este apartamento no está disponible para reservas";
                return RedirectToAction("Details", "Apartments", new { id = apartmentId });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (apartment.UserId == userId)
            {
                TempData["Error"] = "No puedes reservar tu propio apartamento";
                return RedirectToAction("Details", "Apartments", new { id = apartmentId });
            }

            var booking = new Booking
            {
                ApartmentId = apartmentId,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                CreatedAt = DateTime.UtcNow
            };

            ViewBag.Apartment = apartment;
            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("ApartmentId,StartDate,EndDate,NumberOfGuests")] Booking booking)
        {
            try
            {
                var apartment = await _context.Apartment.FindAsync(booking.ApartmentId);
                if (apartment == null || !apartment.IsAvailable)
                {
                    return NotFound();
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (apartment.UserId == userId)
                {
                    return Forbid();
                }

                // Validaciones adicionales
                if (booking.StartDate < DateTime.Today)
                {
                    ModelState.AddModelError("StartDate", "La fecha de inicio no puede ser en el pasado");
                }
                if (booking.EndDate <= booking.StartDate)
                {
                    ModelState.AddModelError("EndDate", "La fecha de fin debe ser posterior a la fecha de inicio");
                }
                if (booking.NumberOfGuests > apartment.MaxOccupancy)
                {
                    ModelState.AddModelError("NumberOfGuests", "El número de huéspedes excede la capacidad máxima");
                }

                if (ModelState.IsValid)
                {
                    booking.UserId = userId;
                    booking.CreatedAt = DateTime.UtcNow;

                    // Calcular el precio total
                    var days = (booking.EndDate - booking.StartDate).Days;
                    booking.TotalPrice = days * apartment.PricePerNight;

                    _context.Add(booking);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", "Apartments", new { id = booking.ApartmentId });
                }

                ViewBag.Apartment = apartment;
                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la reserva");
                ModelState.AddModelError("", "Ocurrió un error al procesar la reserva");
                var apartment = await _context.Apartment.FindAsync(booking.ApartmentId);
                ViewBag.Apartment = apartment;
                return View(booking);
            }
        }

        // GET: Bookings/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Apartment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Verificar que el usuario es el dueño de la reserva
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (booking.UserId != userId)
            {
                return Forbid();
            }

            // Verificar que la reserva no está en el pasado
            if (booking.StartDate < DateTime.Today)
            {
                TempData["Error"] = "No se pueden editar reservas pasadas";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Apartment = booking.Apartment;
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,ApartmentId,StartDate,EndDate,NumberOfGuests")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            try
            {
                var existingBooking = await _context.Booking
                    .Include(b => b.Apartment)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.BookingId == id);

                if (existingBooking == null)
                {
                    return NotFound();
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (existingBooking.UserId != userId)
                {
                    return Forbid();
                }

                // Validaciones
                if (booking.StartDate < DateTime.Today)
                {
                    ModelState.AddModelError("StartDate", "La fecha de inicio no puede ser en el pasado");
                }
                if (booking.EndDate <= booking.StartDate)
                {
                    ModelState.AddModelError("EndDate", "La fecha de fin debe ser posterior a la fecha de inicio");
                }
                if (booking.NumberOfGuests > existingBooking.Apartment.MaxOccupancy)
                {
                    ModelState.AddModelError("NumberOfGuests", "El número de huéspedes excede la capacidad máxima");
                }

                if (ModelState.IsValid)
                {
                    // Mantener los datos que no deben cambiar
                    booking.UserId = existingBooking.UserId;
                    booking.CreatedAt = existingBooking.CreatedAt;
                    booking.UpdatedAt = DateTime.UtcNow;

                    // Recalcular el precio total
                    var days = (booking.EndDate - booking.StartDate).Days;
                    booking.TotalPrice = days * existingBooking.Apartment.PricePerNight;

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Apartment = existingBooking.Apartment;
                return View(booking);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.BookingId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Bookings/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Apartment)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (booking.UserId != userId)
            {
                return Forbid();
            }

            // Verificar si la reserva está próxima a iniciar
            if (booking.StartDate <= DateTime.Today.AddDays(2))
            {
                TempData["Error"] = "No se pueden cancelar reservas que comienzan en menos de 48 horas";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking
                .Include(b => b.Apartment)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (booking.UserId != userId)
            {
                return Forbid();
            }

            // Verificar si la reserva está próxima a iniciar
            if (booking.StartDate <= DateTime.Today.AddDays(2))
            {
                TempData["Error"] = "No se pueden cancelar reservas que comienzan en menos de 48 horas";
                return RedirectToAction(nameof(Index));
            }

            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }

        // Método auxiliar para verificar disponibilidad
        private async Task<bool> IsApartmentAvailable(int apartmentId, DateTime startDate, DateTime endDate, int? excludeBookingId = null)
        {
            var overlappingBookings = await _context.Booking
                .Where(b => b.ApartmentId == apartmentId
                    && b.BookingId != excludeBookingId
                    && ((b.StartDate <= startDate && b.EndDate > startDate)
                        || (b.StartDate < endDate && b.EndDate >= endDate)
                        || (b.StartDate >= startDate && b.EndDate <= endDate)))
                .AnyAsync();

            return !overlappingBookings;
        }
    }
}