using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoMLHOMP.Data;
using ProyectoMLHOMP.Models;
using ProyectoMLHOMP.Services.Interfaces;
using ProyectoMLHOMP.ViewModels;

namespace ProyectoMLHOMP.Services
{
    public class BookingService : IBookingService
    {
        private readonly DatabaseProyecto _context;

        public BookingService(DatabaseProyecto context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync()
        {
            var bookings = await _context.Booking
                .Include(b => b.User)
                .Include(b => b.Apartment)
                .ToListAsync();
            return bookings.Select(MapBookingToViewModel);
        }

        public async Task<BookingViewModel> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Booking
                .Include(b => b.User)
                .Include(b => b.Apartment)
                .FirstOrDefaultAsync(b => b.Id == id);
            return booking != null ? MapBookingToViewModel(booking) : null;
        }

        public async Task<BookingViewModel> CreateBookingAsync(BookingViewModel bookingViewModel)
        {
            var booking = new Booking
            {
                ApartmentId = bookingViewModel.ApartmentId,
                UserId = bookingViewModel.UserId,
                CheckInDate = bookingViewModel.CheckInDate,
                CheckOutDate = bookingViewModel.CheckOutDate,
                TotalPrice = bookingViewModel.TotalPrice,
                Status = BookingStatus.Pending,
                NumberOfGuests = bookingViewModel.NumberOfGuests,
                SpecialRequests = bookingViewModel.SpecialRequests,
                CreatedAt = DateTime.UtcNow
            };

            _context.Booking.Add(booking);
            await _context.SaveChangesAsync();

            return MapBookingToViewModel(booking);
        }

        public async Task<BookingViewModel> UpdateBookingAsync(int id, BookingViewModel bookingViewModel)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return null;
            }

            booking.CheckInDate = bookingViewModel.CheckInDate;
            booking.CheckOutDate = bookingViewModel.CheckOutDate;
            booking.TotalPrice = bookingViewModel.TotalPrice;
            booking.Status = bookingViewModel.Status;
            booking.NumberOfGuests = bookingViewModel.NumberOfGuests;
            booking.SpecialRequests = bookingViewModel.SpecialRequests;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapBookingToViewModel(booking);
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return false;
            }

            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<BookingViewModel>> GetBookingsByUserIdAsync(string userId)
        {
            var bookings = await _context.Booking
                .Where(b => b.UserId == userId)
                .Include(b => b.Apartment)
                .ToListAsync();
            return bookings.Select(MapBookingToViewModel);
        }

        public async Task<IEnumerable<BookingViewModel>> GetBookingsByApartmentIdAsync(int apartmentId)
        {
            var bookings = await _context.Booking
                .Where(b => b.ApartmentId == apartmentId)
                .Include(b => b.User)
                .ToListAsync();
            return bookings.Select(MapBookingToViewModel);
        }

        public async Task<bool> IsApartmentAvailableForBookingAsync(int apartmentId, DateTime checkIn, DateTime checkOut)
        {
            return !await _context.Booking
                .AnyAsync(b => b.ApartmentId == apartmentId &&
                               b.Status != BookingStatus.Cancelled &&
                               ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                                (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                                (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));
        }

        public async Task<decimal> CalculateBookingTotalPriceAsync(int apartmentId, DateTime checkIn, DateTime checkOut)
        {
            var apartment = await _context.Apartment.FindAsync(apartmentId);
            if (apartment == null)
            {
                throw new ArgumentException("Apartment not found", nameof(apartmentId));
            }

            var nights = (checkOut - checkIn).Days;
            return apartment.PricePerNight * nights;
        }

        public async Task<BookingViewModel> ConfirmBookingAsync(int bookingId)
        {
            var booking = await _context.Booking.FindAsync(bookingId);
            if (booking == null)
            {
                return null;
            }

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapBookingToViewModel(booking);
        }

        public async Task<BookingViewModel> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Booking.FindAsync(bookingId);
            if (booking == null)
            {
                return null;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapBookingToViewModel(booking);
        }

        public async Task<IEnumerable<BookingViewModel>> GetUpcomingBookingsAsync(string userId)
        {
            var bookings = await _context.Booking
                .Where(b => b.UserId == userId && b.CheckInDate > DateTime.UtcNow && b.Status != BookingStatus.Cancelled)
                .Include(b => b.Apartment)
                .OrderBy(b => b.CheckInDate)
                .ToListAsync();
            return bookings.Select(MapBookingToViewModel);
        }

        public async Task<IEnumerable<BookingViewModel>> GetPastBookingsAsync(string userId)
        {
            var bookings = await _context.Booking
                .Where(b => b.UserId == userId && b.CheckOutDate < DateTime.UtcNow)
                .Include(b => b.Apartment)
                .OrderByDescending(b => b.CheckOutDate)
                .ToListAsync();
            return bookings.Select(MapBookingToViewModel);
        }

        private BookingViewModel MapBookingToViewModel(Booking booking)
        {
            return new BookingViewModel
            {
                Id = booking.Id,
                ApartmentId = booking.ApartmentId,
                ApartmentTitle = booking.Apartment?.Title,
                UserId = booking.UserId,
                UserName = booking.User?.UserName,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                NumberOfGuests = booking.NumberOfGuests,
                SpecialRequests = booking.SpecialRequests,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }
    }
}