using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMLHOMP.Models
{
    public class Apartment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerNight { get; set; }

        [Required]
        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public int Bedrooms { get; set; }

        public int Bathrooms { get; set; }

        public int MaxOccupancy { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Relación con el propietario (User)
        [Required]
        public string OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        // Relación con las reservas
        public ICollection<Booking> Bookings { get; set; }

        // Relación con las reseñas
        public ICollection<Review> Reviews { get; set; }

        // Constructor
        public Apartment()
        {
            CreatedAt = DateTime.UtcNow;
            Bookings = new List<Booking>();
            Reviews = new List<Review>();
        }
    }
}