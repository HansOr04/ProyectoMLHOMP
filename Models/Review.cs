using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoMLHOMP.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int ApartmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int OverallRating { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Content { get; set; }

        [Range(1, 5)]
        public int CleanlinessRating { get; set; }

        [Range(1, 5)]
        public int CommunicationRating { get; set; }

        [Range(1, 5)]
        public int CheckInRating { get; set; }

        [Range(1, 5)]
        public int AccuracyRating { get; set; }

        [Range(1, 5)]
        public int LocationRating { get; set; }

        [Range(1, 5)]
        public int ValueRating { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public bool IsApproved { get; set; }

        // Relaciones
        public Apartment Apartment { get; set; }
        public User User { get; set; }
    }
}
