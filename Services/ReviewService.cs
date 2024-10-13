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
    public class ReviewService : IReviewService
    {
        private readonly DatabaseProyecto _context;

        public ReviewService(DatabaseProyecto context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewViewModel>> GetAllReviewsAsync()
        {
            var reviews = await _context.Review.Include(r => r.User).Include(r => r.Apartment).ToListAsync();
            return reviews.Select(MapReviewToViewModel);
        }

        public async Task<ReviewViewModel> GetReviewByIdAsync(int id)
        {
            var review = await _context.Review
                .Include(r => r.User)
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.Id == id);
            return review != null ? MapReviewToViewModel(review) : null;
        }

        public async Task<ReviewViewModel> CreateReviewAsync(ReviewViewModel reviewViewModel)
        {
            var review = new Review
            {
                ApartmentId = reviewViewModel.ApartmentId,
                UserId = reviewViewModel.UserId,
                Rating = reviewViewModel.Rating,
                Comment = reviewViewModel.Comment,
                Title = reviewViewModel.Title,
                CreatedAt = DateTime.UtcNow,
                BookingId = reviewViewModel.BookingId,
                CleanlinessRating = reviewViewModel.CleanlinessRating,
                CommunicationRating = reviewViewModel.CommunicationRating,
                CheckInRating = reviewViewModel.CheckInRating,
                AccuracyRating = reviewViewModel.AccuracyRating,
                LocationRating = reviewViewModel.LocationRating,
                ValueRating = reviewViewModel.ValueRating,
                IsApproved = false
            };

            _context.Review.Add(review);
            await _context.SaveChangesAsync();

            return MapReviewToViewModel(review);
        }

        public async Task<ReviewViewModel> UpdateReviewAsync(int id, ReviewViewModel reviewViewModel)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return null;
            }

            review.Rating = reviewViewModel.Rating;
            review.Comment = reviewViewModel.Comment;
            review.Title = reviewViewModel.Title;
            review.UpdatedAt = DateTime.UtcNow;
            review.CleanlinessRating = reviewViewModel.CleanlinessRating;
            review.CommunicationRating = reviewViewModel.CommunicationRating;
            review.CheckInRating = reviewViewModel.CheckInRating;
            review.AccuracyRating = reviewViewModel.AccuracyRating;
            review.LocationRating = reviewViewModel.LocationRating;
            review.ValueRating = reviewViewModel.ValueRating;

            await _context.SaveChangesAsync();

            return MapReviewToViewModel(review);
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return false;
            }

            _context.Review.Remove(review);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<ReviewViewModel>> GetReviewsByApartmentIdAsync(int apartmentId)
        {
            var reviews = await _context.Review
                .Where(r => r.ApartmentId == apartmentId)
                .Include(r => r.User)
                .ToListAsync();
            return reviews.Select(MapReviewToViewModel);
        }

        public async Task<IEnumerable<ReviewViewModel>> GetReviewsByUserIdAsync(string userId)
        {
            var reviews = await _context.Review
                .Where(r => r.UserId == userId)
                .Include(r => r.Apartment)
                .ToListAsync();
            return reviews.Select(MapReviewToViewModel);
        }

        public async Task<double> GetAverageRatingForApartmentAsync(int apartmentId)
        {
            return await _context.Review
                .Where(r => r.ApartmentId == apartmentId)
                .AverageAsync(r => r.Rating);
        }

        public async Task<bool> HasUserReviewedApartmentAsync(string userId, int apartmentId)
        {
            return await _context.Review
                .AnyAsync(r => r.UserId == userId && r.ApartmentId == apartmentId);
        }

        public async Task<ReviewViewModel> GetUserReviewForApartmentAsync(string userId, int apartmentId)
        {
            var review = await _context.Review
                .Include(r => r.User)
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ApartmentId == apartmentId);
            return review != null ? MapReviewToViewModel(review) : null;
        }

        public async Task<IEnumerable<ReviewViewModel>> GetMostRecentReviewsAsync(int count)
        {
            var reviews = await _context.Review
                .Include(r => r.User)
                .Include(r => r.Apartment)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
            return reviews.Select(MapReviewToViewModel);
        }

        public async Task<IEnumerable<ReviewViewModel>> GetTopRatedReviewsAsync(int count)
        {
            var reviews = await _context.Review
                .Include(r => r.User)
                .Include(r => r.Apartment)
                .OrderByDescending(r => r.Rating)
                .Take(count)
                .ToListAsync();
            return reviews.Select(MapReviewToViewModel);
        }

        private ReviewViewModel MapReviewToViewModel(Review review)
        {
            return new ReviewViewModel
            {
                Id = review.Id,
                ApartmentId = review.ApartmentId,
                ApartmentTitle = review.Apartment?.Title,
                UserId = review.UserId,
                UserName = review.User?.UserName,
                Rating = review.Rating,
                Comment = review.Comment,
                Title = review.Title,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                BookingId = review.BookingId,
                CleanlinessRating = review.CleanlinessRating,
                CommunicationRating = review.CommunicationRating,
                CheckInRating = review.CheckInRating,
                AccuracyRating = review.AccuracyRating,
                LocationRating = review.LocationRating,
                ValueRating = review.ValueRating,
                IsApproved = review.IsApproved
            };
        }
    }
}