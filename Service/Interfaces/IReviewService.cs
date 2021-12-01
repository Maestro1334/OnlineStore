using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IReviewService
    {
        public Task<List<Review>> GetReviews();
        public Task<Review> GetReviewById(Guid id);
        public Task AddReview(Review review);
        public Task UpdateReviewById(Guid id, Review updatedReview);
        public Task DeleteReviewById(Guid id);
    }

}
