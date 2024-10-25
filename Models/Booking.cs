using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMLHOMP.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El número de huéspedes debe ser mayor a 0")]
        public int NumberOfGuests { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio total debe ser mayor a 0")]
        [Column(TypeName = "float")]
        public double TotalPrice { get; set; }

        [Required]
        public int ApartmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Propiedades de navegación
        [ForeignKey("ApartmentId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Apartment? Apartment { get; set; }

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual User? Guest { get; set; }

        public Booking()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}