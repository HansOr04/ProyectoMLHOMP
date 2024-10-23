using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoMLHOMP.Models
{
    public class Apartment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApartmentId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres")]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder los 2000 caracteres")]
        [Display(Name = "Descripción")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio por noche es requerido")]
        [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre $0.01 y $99,999.99")]
        [Display(Name = "Precio por Noche")]
        [DataType(DataType.Currency)]
        public double PricePerNight { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [Display(Name = "Dirección")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es requerida")]
        [Display(Name = "Ciudad")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "El país es requerido")]
        [Display(Name = "País")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de habitaciones es requerido")]
        [Range(1, 20, ErrorMessage = "El número de habitaciones debe estar entre 1 y 20")]
        [Display(Name = "Habitaciones")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "El número de baños es requerido")]
        [Range(1, 20, ErrorMessage = "El número de baños debe estar entre 1 y 20")]
        [Display(Name = "Baños")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "La ocupación máxima es requerida")]
        [Range(1, 50, ErrorMessage = "La ocupación máxima debe estar entre 1 y 50 personas")]
        [Display(Name = "Ocupación Máxima")]
        public int MaxOccupancy { get; set; }

        [Display(Name = "Disponible")]
        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? Owner { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();
    }
}