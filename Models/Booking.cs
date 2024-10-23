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
        public int NumberOfGuests { get; set; }

        [Required]
        [Column(TypeName = "float")]
        public double TotalPrice { get; set; }

        [Required]
        public int ApartmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Relaciones
        [ForeignKey("ApartmentId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual Apartment Apartment { get; set; } = null!;

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public virtual User Guest { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Booking()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}