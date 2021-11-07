using System;
using DAL;
using Domain;

namespace Service
{
    public interface IProductService
    {
    }

    public class ProductService : IProductService
    {
        private readonly OnlineStoreDBContext _context;

        public ProductService(OnlineStoreDBContext context)
        {
            _context = context;
        }
    }
}
