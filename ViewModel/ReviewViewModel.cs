using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoMLHOMP.ViewModels
{
    public class ReviewViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ApartmentId { get; set; }
        public string ApartmentTitle { get; set; }

        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? BookingId { get; set; }

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

        // Propiedades calculadas que pueden ser útiles en la vista
        public double AverageRating
        {
            get
            {
                var ratings = new[]
                {
                    CleanlinessRating, CommunicationRating, CheckInRating,
                    AccuracyRating, LocationRating, ValueRating
                };
                var validRatings = ratings.Where(r => r.HasValue);
                return validRatings.Any() ? validRatings.Average(r => r.Value) : 0;
            }
        }

        public string FormattedCreatedDate => CreatedAt.ToString("dd MMM yyyy");
    }
}