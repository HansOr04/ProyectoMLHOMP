using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoMLHOMP.Models;
using ProyectoMLHOMP.ViewModels;

namespace ProyectoMLHOMP.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewViewModel>> GetAllReviewsAsync();
        Task<ReviewViewModel> GetReviewByIdAsync(int id);
        Task<ReviewViewModel> CreateReviewAsync(ReviewViewModel reviewViewModel);
        Task<ReviewViewModel> UpdateReviewAsync(int id, ReviewViewModel reviewViewModel);
        Task<bool> DeleteReviewAsync(int id);
        Task<IEnumerable<ReviewViewModel>> GetReviewsByApartmentIdAsync(int apartmentId);
        Task<IEnumerable<ReviewViewModel>> GetReviewsByUserIdAsync(string userId);
        Task<double> GetAverageRatingForApartmentAsync(int apartmentId);
        Task<bool> HasUserReviewedApartmentAsync(string userId, int apartmentId);
        Task<ReviewViewModel> GetUserReviewForApartmentAsync(string userId, int apartmentId);
        Task<IEnumerable<ReviewViewModel>> GetMostRecentReviewsAsync(int count);
        Task<IEnumerable<ReviewViewModel>> GetTopRatedReviewsAsync(int count);
    }
}