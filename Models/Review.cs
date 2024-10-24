using ProyectoMLHOMP.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ProyectoMLHOMP.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "El ID del apartamento es requerido")]
        public int ApartmentId { get; set; }

        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es requerido")]
        [StringLength(1000, ErrorMessage = "El contenido no puede exceder los 1000 caracteres")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "La calificación general es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int OverallRating { get; set; }

        [Required(ErrorMessage = "La calificación de limpieza es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int CleanlinessRating { get; set; }

        [Required(ErrorMessage = "La calificación de comunicación es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int CommunicationRating { get; set; }

        [Required(ErrorMessage = "La calificación de check-in es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int CheckInRating { get; set; }

        [Required(ErrorMessage = "La calificación de precisión es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int AccuracyRating { get; set; }

        [Required(ErrorMessage = "La calificación de ubicación es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int LocationRating { get; set; }

        [Required(ErrorMessage = "La calificación de valor es requerida")]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5")]
        public int ValueRating { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsApproved { get; set; }

        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }

        [ForeignKey("UserId")]
        public virtual User? Reviewer { get; set; }
    }
}