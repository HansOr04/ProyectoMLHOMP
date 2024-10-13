using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMLHOMP.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        public Apartment Apartment { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Opcional: Referencia a la reserva asociada
        public int? BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        // Propiedades adicionales para una revisión más detallada
        [Range(1, 5)]
        public int? CleanlinessRating { get; set; }

        [Range(1, 5)]
        public int? CommunicationRating { get; set; }

        [Range(1, 5)]
        public int? CheckInRating { get; set; }

        [Range(1, 5)]
        public int? AccuracyRating { get; set; }

        [Range(1, 5)]
        public int? LocationRating { get; set; }

        [Range(1, 5)]
        public int? ValueRating { get; set; }

        public bool IsApproved { get; set; }

        // Constructor
        public Review()
        {
            CreatedAt = DateTime.UtcNow;
            IsApproved = false; // Por defecto, las reseñas pueden requerir aprobación
        }
    }
}