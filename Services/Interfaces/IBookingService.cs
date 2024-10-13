using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoMLHOMP.Models;
using ProyectoMLHOMP.ViewModels;

namespace ProyectoMLHOMP.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync();
        Task<BookingViewModel> GetBookingByIdAsync(int id);
        Task<BookingViewModel> CreateBookingAsync(BookingViewModel bookingViewModel);
        Task<BookingViewModel> UpdateBookingAsync(int id, BookingViewModel bookingViewModel);
        Task<bool> DeleteBookingAsync(int id);
        Task<IEnumerable<BookingViewModel>> GetBookingsByUserIdAsync(string userId);
        Task<IEnumerable<BookingViewModel>> GetBookingsByApartmentIdAsync(int apartmentId);
        Task<bool> IsApartmentAvailableForBookingAsync(int apartmentId, DateTime checkIn, DateTime checkOut);
        Task<decimal> CalculateBookingTotalPriceAsync(int apartmentId, DateTime checkIn, DateTime checkOut);
        Task<BookingViewModel> ConfirmBookingAsync(int bookingId);
        Task<BookingViewModel> CancelBookingAsync(int bookingId);
        Task<IEnumerable<BookingViewModel>> GetUpcomingBookingsAsync(string userId);
        Task<IEnumerable<BookingViewModel>> GetPastBookingsAsync(string userId);
    }
}