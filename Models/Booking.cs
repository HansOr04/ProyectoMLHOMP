using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMLHOMP.Models
{
    /// <summary>
    /// Representa una reserva de apartamento en el sistema.
    /// </summary>
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
        [Column(TypeName = "double(18,2)")]
        public double TotalPrice { get; set; }

        [Required]
        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        public Aparment Apartment { get; set; }

        [Required]
        public int GuestUserId { get; set; }

        [ForeignKey("GuestUserId")]
        public User Guest { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Booking()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
