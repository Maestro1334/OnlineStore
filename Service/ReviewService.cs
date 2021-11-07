using System;
using DAL;
using Domain;

namespace Service
{
    public interface IReviewService
    {
    }

    public class ReviewService : IReviewService
    {
        private readonly OnlineStoreDBContext _context;

        public ReviewService(OnlineStoreDBContext context)
        {
            _context = context;
        }
    }
}
