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
    public class ApartmentService : IApartmentService
    {
        private readonly DatabaseProyecto _context;

        public ApartmentService(DatabaseProyecto context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApartmentViewModel>> GetAllApartmentsAsync()
        {
            var apartments = await _context.Apartment
                .Include(a => a.Owner)
                .ToListAsync();
            return apartments.Select(MapApartmentToViewModel);
        }

        public async Task<ApartmentViewModel> GetApartmentByIdAsync(int id)
        {
            var apartment = await _context.Apartment
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.Id == id);
            return apartment != null ? MapApartmentToViewModel(apartment) : null;
        }

        public async Task<ApartmentViewModel> CreateApartmentAsync(ApartmentViewModel apartmentViewModel)
        {
            var apartment = new Apartment
            {
                Title = apartmentViewModel.Title,
                Description = apartmentViewModel.Description,
                PricePerNight = apartmentViewModel.PricePerNight,
                Address = apartmentViewModel.Address,
                City = apartmentViewModel.City,
                Country = apartmentViewModel.Country,
                Bedrooms = apartmentViewModel.Bedrooms,
                Bathrooms = apartmentViewModel.Bathrooms,
                MaxOccupancy = apartmentViewModel.MaxOccupancy,
                IsAvailable = apartmentViewModel.IsAvailable,
                ImageUrl = apartmentViewModel.ImageUrl,
                OwnerId = apartmentViewModel.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Apartment.Add(apartment);
            await _context.SaveChangesAsync();

            return MapApartmentToViewModel(apartment);
        }

        public async Task<ApartmentViewModel> UpdateApartmentAsync(int id, ApartmentViewModel apartmentViewModel)
        {
            var apartment = await _context.Apartment.FindAsync(id);
            if (apartment == null)
            {
                return null;
            }

            apartment.Title = apartmentViewModel.Title;
            apartment.Description = apartmentViewModel.Description;
            apartment.PricePerNight = apartmentViewModel.PricePerNight;
            apartment.Address = apartmentViewModel.Address;
            apartment.City = apartmentViewModel.City;
            apartment.Country = apartmentViewModel.Country;
            apartment.Bedrooms = apartmentViewModel.Bedrooms;
            apartment.Bathrooms = apartmentViewModel.Bathrooms;
            apartment.MaxOccupancy = apartmentViewModel.MaxOccupancy;
            apartment.IsAvailable = apartmentViewModel.IsAvailable;
            apartment.ImageUrl = apartmentViewModel.ImageUrl;
            apartment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapApartmentToViewModel(apartment);
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            var apartment = await _context.Apartment.FindAsync(id);
            if (apartment == null)
            {
                return false;
            }

            _context.Apartment.Remove(apartment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ApartmentViewModel>> SearchApartmentsAsync(string searchTerm, DateTime? checkIn, DateTime? checkOut, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Apartment.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => a.Title.Contains(searchTerm) || a.Description.Contains(searchTerm) || a.City.Contains(searchTerm) || a.Country.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(a => a.PricePerNight >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(a => a.PricePerNight <= maxPrice.Value);
            }

            if (checkIn.HasValue && checkOut.HasValue)
            {
                query = query.Where(a => !a.Bookings.Any(b =>
                    (checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                    (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                    (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));
            }

            var apartments = await query.Include(a => a.Owner).ToListAsync();
            return apartments.Select(MapApartmentToViewModel);
        }

        public async Task<bool> IsApartmentAvailableAsync(int id, DateTime checkIn, DateTime checkOut)
        {
            return !await _context.Booking
                .AnyAsync(b => b.ApartmentId == id &&
                               b.Status != BookingStatus.Cancelled &&
                               ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                                (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                                (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));
        }

        public async Task<IEnumerable<ReviewViewModel>> GetApartmentReviewsAsync(int apartmentId)
        {
            var reviews = await _context.Review
                .Where(r => r.ApartmentId == apartmentId)
                .Include(r => r.User)
                .ToListAsync();

            return reviews.Select(r => new ReviewViewModel
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UserName = r.User.UserName
                // Añade aquí más propiedades según sea necesario
            });
        }

        public async Task<double> GetApartmentAverageRatingAsync(int apartmentId)
        {
            return await _context.Review
                .Where(r => r.ApartmentId == apartmentId)
                .AverageAsync(r => r.Rating);
        }

        public async Task<IEnumerable<ApartmentViewModel>> GetApartmentsByOwnerIdAsync(string ownerId)
        {
            var apartments = await _context.Apartment
                .Where(a => a.OwnerId == ownerId)
                .Include(a => a.Owner)
                .ToListAsync();
            return apartments.Select(MapApartmentToViewModel);
        }

        private ApartmentViewModel MapApartmentToViewModel(Apartment apartment)
        {
            return new ApartmentViewModel
            {
                Id = apartment.Id,
                Title = apartment.Title,
                Description = apartment.Description,
                PricePerNight = apartment.PricePerNight,
                Address = apartment.Address,
                City = apartment.City,
                Country = apartment.Country,
                Bedrooms = apartment.Bedrooms,
                Bathrooms = apartment.Bathrooms,
                MaxOccupancy = apartment.MaxOccupancy,
                IsAvailable = apartment.IsAvailable,
                ImageUrl = apartment.ImageUrl,
                OwnerId = apartment.OwnerId,
                OwnerName = apartment.Owner?.UserName,
                CreatedAt = apartment.CreatedAt,
                UpdatedAt = apartment.UpdatedAt
            };
        }
    }
}