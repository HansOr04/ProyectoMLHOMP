using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMLHOMP.Models
{
    public class Apartment
    {
        [Key]
        public int ApartmentId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "La descripción es requerida")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "El precio por noche es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0")]
        public double PricePerNight { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        public string Address { get; set; } = "";

        [Required(ErrorMessage = "La ciudad es requerida")]
        public string City { get; set; } = "";

        [Required(ErrorMessage = "El país es requerido")]
        public string Country { get; set; } = "";

        [Required(ErrorMessage = "El número de habitaciones es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de habitaciones debe ser al menos 1")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "El número de baños es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de baños debe ser al menos 1")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "La ocupación máxima es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La ocupación máxima debe ser al menos 1")]
        public int MaxOccupancy { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required(ErrorMessage = "El ID del propietario es requerido")]
        public int OwnerUserId { get; set; }

        [ForeignKey("OwnerUserId")]
        public User? Owner { get; set; }

        public ICollection<Booking>? Bookings { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}