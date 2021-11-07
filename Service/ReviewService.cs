using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public interface IReviewService
    {
        public Task<List<Review>> GetReviews();
        public Task<Review> GetReviewById(Guid id);
        public Task AddReview(Review review);
        public Task UpdateReviewById(Guid id, Review updatedReview);
        public Task DeleteReviewById(Guid id);
    }

    public class ReviewService : IReviewService
    {
        private readonly OnlineStoreDBContext _context;

        public ReviewService(OnlineStoreDBContext context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        public async Task<Review> GetReviewById(Guid id)
        {
            return await _context.Reviews.FindAsync(id);
        }

        public async Task AddReview(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReviewById(Guid id, Review updatedReview)
        {
            updatedReview.Id = id;
            _context.Reviews.Update(updatedReview);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteReviewById(Guid id)
        {
            _context.Reviews.Remove(new Review { Id = id });
            await _context.SaveChangesAsync();
        }
    }
}
