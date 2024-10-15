using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMLHOMP.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Relación con el apartamento
        [Required]
        public int ApartmentId { get; set; }
        [ForeignKey("ApartmentId")]
        public Apartment Apartment { get; set; }

        // Relación con el usuario (quien hace la reserva)
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Constructor
        public Booking()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}