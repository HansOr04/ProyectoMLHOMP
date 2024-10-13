using System;
using System.ComponentModel.DataAnnotations;
using ProyectoMLHOMP.Models;

namespace ProyectoMLHOMP.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ApartmentId { get; set; }
        public string ApartmentTitle { get; set; }

        [Required]
        public string UserId { get; set; }
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El número de huéspedes debe ser al menos 1.")]
        public int NumberOfGuests { get; set; }

        [StringLength(500)]
        public string SpecialRequests { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Propiedades calculadas que pueden ser útiles en la vista
        public int NumberOfNights => (CheckOutDate - CheckInDate).Days;

        public string FormattedCheckInDate => CheckInDate.ToString("dd MMM yyyy");
        public string FormattedCheckOutDate => CheckOutDate.ToString("dd MMM yyyy");

        public string StatusDisplay => Status.ToString();

        public bool CanBeCancelled => Status == BookingStatus.Pending || Status == BookingStatus.Confirmed;

        public bool IsPastBooking => CheckOutDate < DateTime.UtcNow;

        public bool IsCurrentBooking => CheckInDate <= DateTime.UtcNow && CheckOutDate >= DateTime.UtcNow;

        public bool IsFutureBooking => CheckInDate > DateTime.UtcNow;
    }
}