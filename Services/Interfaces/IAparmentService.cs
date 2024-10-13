using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoMLHOMP.Models;
using ProyectoMLHOMP.ViewModels;

namespace ProyectoMLHOMP.Services.Interfaces
{
    public interface IApartmentService
    {
        Task<IEnumerable<ApartmentViewModel>> GetAllApartmentsAsync();
        Task<ApartmentViewModel> GetApartmentByIdAsync(int id);
        Task<ApartmentViewModel> CreateApartmentAsync(ApartmentViewModel apartmentViewModel);
        Task<ApartmentViewModel> UpdateApartmentAsync(int id, ApartmentViewModel apartmentViewModel);
        Task<bool> DeleteApartmentAsync(int id);
        Task<IEnumerable<ApartmentViewModel>> SearchApartmentsAsync(string searchTerm, DateTime? checkIn, DateTime? checkOut, decimal? minPrice, decimal? maxPrice);
        Task<bool> IsApartmentAvailableAsync(int id, DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<ReviewViewModel>> GetApartmentReviewsAsync(int apartmentId);
        Task<double> GetApartmentAverageRatingAsync(int apartmentId);
        Task<IEnumerable<ApartmentViewModel>> GetApartmentsByOwnerIdAsync(string ownerId);
    }
}