using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ProyectoMLHOMP.Models;

namespace ProyectoMLHOMP.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ProyectoContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ProyectoContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Bookings - Lista todas las reservas del usuario
        public async Task<IActionResult> Index()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las reservas");
                TempData["Error"] = "Ocurrió un error al cargar las reservas";
                return View(new List<Booking>());
            }
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var booking = await _context.Booking
                    .Include(b => b.Apartment)
                    .Include(b => b.Guest)
                    .FirstOrDefaultAsync(m => m.BookingId == id);

                if (booking == null)
                {
                    _logger.LogWarning("Reserva {BookingId} no encontrada", id);
                    return NotFound();
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (booking.UserId != userId && booking.Apartment.UserId != userId)
                {
                    _logger.LogWarning("Usuario {UserId} intentó acceder a la reserva {BookingId} sin autorización", userId, id);
                    return Forbid();
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los detalles de la reserva {BookingId}", id);
                TempData["Error"] = "Error al cargar los detalles de la reserva";
                return RedirectToAction("Index", "Apartments");
            }
        }

        // GET: Bookings/Create/5 (5 es el ID del apartamento)
        // GET: Bookings/Create
        // Modificamos para aceptar tanto id como apartmentId
        public async Task<IActionResult> Create(int? id, int? apartmentId)
        {
            // Usamos el que no sea null, dando preferencia a id
            int? targetApartmentId = id ?? apartmentId;

            _logger.LogInformation("Iniciando proceso de reserva para apartamento {ApartmentId}", targetApartmentId);

            if (targetApartmentId == null)
            {
                _logger.LogWarning("Intento de crear reserva sin ID de apartamento");
                return BadRequest("Se requiere un ID de apartamento válido");
            }

            try
            {
                var apartment = await _context.Apartment
                    .Include(a => a.Owner)
                    .Include(a => a.Bookings)
                    .FirstOrDefaultAsync(a => a.ApartmentId == targetApartmentId);

                if (apartment == null)
                {
                    _logger.LogWarning("Apartamento {ApartmentId} no encontrado", targetApartmentId);
                    return NotFound("El apartamento especificado no existe");
                }

                if (!apartment.IsAvailable)
                {
                    _logger.LogWarning("Intento de reservar apartamento no disponible {ApartmentId}", targetApartmentId);
                    TempData["Error"] = "Este apartamento no está disponible para reservas";
                    return RedirectToAction("Details", "Apartments", new { id = targetApartmentId });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (apartment.UserId == userId)
                {
                    _logger.LogWarning("Usuario {UserId} intentó reservar su propio apartamento {ApartmentId}", userId, targetApartmentId);
                    TempData["Error"] = "No puedes reservar tu propio apartamento";
                    return RedirectToAction("Details", "Apartments", new { id = targetApartmentId });
                }

                var booking = new Booking
                {
                    ApartmentId = apartment.ApartmentId,
                    UserId = userId,
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(2),
                    NumberOfGuests = 1,
                    TotalPrice = apartment.PricePerNight,
                    CreatedAt = DateTime.UtcNow
                };

                ViewData["Apartment"] = apartment;
                ViewData["MaxGuests"] = apartment.MaxOccupancy;
                ViewData["PricePerNight"] = apartment.PricePerNight;
                ViewData["MinStartDate"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                ViewData["MaxStartDate"] = DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd");

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar la creación de reserva para apartamento {ApartmentId}", targetApartmentId);
                TempData["Error"] = "Ocurrió un error al procesar tu solicitud";
                return RedirectToAction("Index", "Apartments");
            }
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApartmentId,StartDate,EndDate,NumberOfGuests,TotalPrice")] Booking booking)
        {
            _logger.LogInformation("Procesando creación de reserva para apartamento {ApartmentId}", booking.ApartmentId);

            try
            {
                var apartment = await _context.Apartment
                    .FirstOrDefaultAsync(a => a.ApartmentId == booking.ApartmentId);

                if (apartment == null)
                {
                    _logger.LogWarning("Apartamento {ApartmentId} no encontrado", booking.ApartmentId);
                    return NotFound();
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                // Validaciones
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
                    ModelState.AddModelError("NumberOfGuests", $"El número de huéspedes no puede superar {apartment.MaxOccupancy}");
                }

                // Verificar que el precio total sea correcto
                var nights = (booking.EndDate - booking.StartDate).Days;
                var expectedTotalPrice = nights * apartment.PricePerNight;

                if (Math.Abs(booking.TotalPrice - expectedTotalPrice) > 0.01) // Pequeña tolerancia para errores de redondeo
                {
                    booking.TotalPrice = expectedTotalPrice; // Corregir el precio si no coincide
                }

                // Verificar disponibilidad
                var existingBooking = await _context.Booking
                    .AnyAsync(b => b.ApartmentId == booking.ApartmentId &&
                                  ((b.StartDate <= booking.StartDate && b.EndDate > booking.StartDate) ||
                                   (b.StartDate < booking.EndDate && b.EndDate >= booking.EndDate) ||
                                   (b.StartDate >= booking.StartDate && b.EndDate <= booking.EndDate)));

                if (existingBooking)
                {
                    ModelState.AddModelError("", "El apartamento no está disponible en las fechas seleccionadas");
                }

                if (ModelState.IsValid)
                {
                    booking.UserId = userId;
                    booking.CreatedAt = DateTime.UtcNow;

                    await _context.Booking.AddAsync(booking);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Reserva creada exitosamente para apartamento {ApartmentId}", booking.ApartmentId);
                    TempData["Success"] = "Reserva creada exitosamente";
                    return RedirectToAction("Index", "Apartments");
                }

                // Si hay errores, recargar la vista
                ViewData["Apartment"] = apartment;
                ViewData["MaxGuests"] = apartment.MaxOccupancy;
                ViewData["PricePerNight"] = apartment.PricePerNight;
                ViewData["MinStartDate"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                ViewData["MaxStartDate"] = DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd");

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva para apartamento {ApartmentId}", booking.ApartmentId);
                ModelState.AddModelError("", "Ocurrió un error al procesar la reserva");

                var apartment = await _context.Apartment.FindAsync(booking.ApartmentId);
                ViewData["Apartment"] = apartment;
                ViewData["MaxGuests"] = apartment?.MaxOccupancy;
                ViewData["PricePerNight"] = apartment?.PricePerNight;
                ViewData["MinStartDate"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                ViewData["MaxStartDate"] = DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd");

                return View(booking);
            }
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
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

                if (booking.StartDate < DateTime.Today)
                {
                    TempData["Error"] = "No se pueden editar reservas pasadas";
                    return RedirectToAction("Index", "Apartments");
                }

                ViewData["Apartment"] = booking.Apartment;
                ViewData["MaxGuests"] = booking.Apartment.MaxOccupancy;
                ViewData["PricePerNight"] = booking.Apartment.PricePerNight;
                ViewData["MinStartDate"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                ViewData["MaxStartDate"] = DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd");

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la edición de la reserva {BookingId}", id);
                TempData["Error"] = "Error al cargar la reserva";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
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
                    booking.UserId = existingBooking.UserId;
                    booking.CreatedAt = existingBooking.CreatedAt;
                    booking.UpdatedAt = DateTime.UtcNow;

                    var days = (booking.EndDate - booking.StartDate).Days;
                    booking.TotalPrice = days * existingBooking.Apartment.PricePerNight;

                    _context.Update(booking);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Reserva {BookingId} actualizada exitosamente", id);
                    TempData["Success"] = "Reserva actualizada exitosamente";
                    return RedirectToAction("Index", "Apartments");
                }

                ViewData["Apartment"] = existingBooking.Apartment;
                ViewData["MaxGuests"] = existingBooking.Apartment.MaxOccupancy;
                ViewData["PricePerNight"] = existingBooking.Apartment.PricePerNight;
                ViewData["MinStartDate"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
                ViewData["MaxStartDate"] = DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd");

                return View(booking);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await BookingExists(booking.BookingId))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Error de concurrencia al actualizar la reserva {BookingId}", id);
                    throw;
                }
            }
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
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

                if (booking.StartDate <= DateTime.Today.AddDays(2))
                {
                    TempData["Error"] = "No se pueden cancelar reservas que comienzan en menos de 48 horas";
                    return RedirectToAction("Index", "Apartments");
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la vista de eliminación para la reserva {BookingId}", id);
                TempData["Error"] = "Error al procesar la solicitud de cancelación";
                return RedirectToAction("Index", "Apartments");
            }
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
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

                if (booking.StartDate <= DateTime.Today.AddDays(2))
                {
                    TempData["Error"] = "No se pueden cancelar reservas que comienzan en menos de 48 horas";
                    return RedirectToAction("Index", "Apartments");
                }

                _context.Booking.Remove(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva {BookingId} cancelada exitosamente", id);
                TempData["Success"] = "Reserva cancelada exitosamente";
                return RedirectToAction("Index", "Apartments"); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la reserva {BookingId}", id);
                TempData["Error"] = "Error al cancelar la reserva";
                return RedirectToAction("Index", "Apartments");
            }
        }

        private async Task<bool> BookingExists(int id)
        {
            return await _context.Booking.AnyAsync(e => e.BookingId == id);
        }

        private async Task<bool> IsApartmentAvailable(int apartmentId, DateTime startDate, DateTime endDate, int? excludeBookingId = null)
        {
            return !await _context.Booking
                .AnyAsync(b => b.ApartmentId == apartmentId
                    && b.BookingId != excludeBookingId
                    && ((b.StartDate <= startDate && b.EndDate > startDate)
                        || (b.StartDate < endDate && b.EndDate >= endDate)
                        || (b.StartDate >= startDate && b.EndDate <= endDate)));
        }
    }
}