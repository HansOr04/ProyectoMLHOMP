using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ProyectoMLHOMP.Models;

namespace ProyectoMLHOMP.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ProyectoContext _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ProyectoContext context, ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [Authorize]
        public async Task<IActionResult> Create(int? apartmentId)
        {
            if (!apartmentId.HasValue)
            {
                _logger.LogWarning("Intento de crear reseña sin ID de apartamento");
                return BadRequest("Se requiere un ID de apartamento válido");
            }

            _logger.LogInformation($"Iniciando creación de reseña para apartamento {apartmentId}");

            try
            {
                // Verificar si el apartamento existe
                var apartment = await _context.Apartment
                    .Include(a => a.Owner)
                    .FirstOrDefaultAsync(a => a.ApartmentId == apartmentId);

                if (apartment == null)
                {
                    _logger.LogWarning($"Apartamento {apartmentId} no encontrado");
                    TempData["Error"] = "El apartamento especificado no existe";
                    return RedirectToAction("Index", "Home");
                }

                // Verificar autenticación
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return RedirectToAction("Login", "Users",
                        new { returnUrl = Url.Action("Create", "Reviews", new { apartmentId = apartmentId }) });
                }

                // Obtener ID del usuario
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                {
                    _logger.LogError("Error al obtener el ID del usuario");
                    TempData["Error"] = "Error al verificar la identidad del usuario";
                    return RedirectToAction("Details", "Apartments", new { id = apartmentId });
                }

                // Verificar si ya existe una reseña
                var existingReview = await _context.Review
                    .AnyAsync(r => r.ApartmentId == apartmentId && r.UserId == userId);

                if (existingReview)
                {
                    TempData["Error"] = "Ya has escrito una reseña para este apartamento";
                    return RedirectToAction("Details", "Apartments", new { id = apartmentId });
                }

                // Crear el modelo para la vista
                var review = new Review
                {
                    ApartmentId = apartmentId.Value,
                    CreatedDate = DateTime.UtcNow
                };

                ViewData["ApartmentTitle"] = apartment.Title;
                ViewData["ApartmentId"] = apartmentId;
                ViewData["IsOwner"] = (apartment.UserId == userId);

                return View(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al preparar el formulario de reseña para el apartamento {apartmentId}");
                TempData["Error"] = "Ocurrió un error al cargar el formulario. Por favor, intenta nuevamente.";
                return RedirectToAction("Details", "Apartments", new { id = apartmentId });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("ApartmentId,Title,Content,OverallRating,CleanlinessRating,CommunicationRating,CheckInRating,AccuracyRating,LocationRating,ValueRating")] Review review)
        {
            if (review.ApartmentId <= 0)
            {
                _logger.LogError("Intento de crear reseña con ID de apartamento inválido");
                return BadRequest("ID de apartamento inválido");
            }

            try
            {
                // Verificar el modelo
                if (!ModelState.IsValid)
                {
                    var apartmentDetails = await _context.Apartment.FindAsync(review.ApartmentId);
                    if (apartmentDetails == null)
                    {
                        return NotFound("Apartamento no encontrado");
                    }

                    ViewData["ApartmentTitle"] = apartmentDetails.Title;
                    ViewData["ApartmentId"] = review.ApartmentId;

                    var modelErrors = string.Join(" | ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning($"Modelo inválido: {modelErrors}");

                    return View(review);
                }

                // Verificar autenticación y obtener ID del usuario
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                {
                    _logger.LogError("Error al obtener el ID del usuario");
                    return RedirectToAction("Login", "Users");
                }

                // Verificar si el apartamento existe
                var targetApartment = await _context.Apartment
                    .FirstOrDefaultAsync(a => a.ApartmentId == review.ApartmentId);

                if (targetApartment == null)
                {
                    _logger.LogError($"Apartamento {review.ApartmentId} no encontrado al intentar crear reseña");
                    TempData["Error"] = "El apartamento especificado no existe";
                    return RedirectToAction("Index", "Home");
                }

                // Verificar si ya existe una reseña
                var existingReview = await _context.Review
                    .AnyAsync(r => r.ApartmentId == review.ApartmentId && r.UserId == userId);

                if (existingReview)
                {
                    TempData["Error"] = "Ya has escrito una reseña para este apartamento";
                    return RedirectToAction("Details", "Apartments", new { id = review.ApartmentId });
                }

                // Preparar la reseña
                var isOwner = targetApartment.UserId == userId;

                review.UserId = userId;
                review.CreatedDate = DateTime.UtcNow;
                review.IsApproved = isOwner;

                // Guardar la reseña
                await _context.Review.AddAsync(review);
                var saveResult = await _context.SaveChangesAsync();

                if (saveResult > 0)
                {
                    TempData["Success"] = "Tu reseña ha sido publicada correctamente";
                    _logger.LogInformation($"Reseña creada exitosamente para el apartamento {review.ApartmentId}");
                    return RedirectToAction("Details", "Apartments", new { id = review.ApartmentId });
                }
                else
                {
                    throw new Exception("No se pudo guardar la reseña en la base de datos");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear reseña para apartamento {review.ApartmentId}");
                ModelState.AddModelError("", "Ocurrió un error al guardar la reseña. Por favor, intenta nuevamente.");

                var currentApartment = await _context.Apartment.FindAsync(review.ApartmentId);
                ViewData["ApartmentTitle"] = currentApartment?.Title;
                ViewData["ApartmentId"] = review.ApartmentId;

                return View(review);
            }
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || review.UserId != int.Parse(userId))
            {
                return Forbid();
            }

            ViewData["ApartmentTitle"] = review.Apartment?.Title;
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,ApartmentId,Title,Content,OverallRating,CleanlinessRating,CommunicationRating,CheckInRating,AccuracyRating,LocationRating,ValueRating")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            try
            {
                var existingReview = await _context.Review
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (existingReview == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId) || existingReview.UserId != int.Parse(userId))
                {
                    return Forbid();
                }

                review.UserId = existingReview.UserId;
                review.CreatedDate = existingReview.CreatedDate;
                review.UpdatedDate = DateTime.UtcNow;
                review.IsApproved = existingReview.IsApproved;

                _context.Update(review);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Apartments", new { id = review.ApartmentId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Review.AnyAsync(r => r.ReviewId == review.ReviewId))
                {
                    return NotFound();
                }
                throw;
            }
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || review.UserId != int.Parse(userId))
            {
                return Forbid();
            }

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || review.UserId != int.Parse(userId))
            {
                return Forbid();
            }

            var apartmentId = review.ApartmentId;
            _context.Review.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Apartments", new { id = apartmentId });
        }
    }
}