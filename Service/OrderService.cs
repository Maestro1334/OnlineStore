using System;
using DAL;
using Domain;

namespace Service
{
    public interface IOrderService
    {

    }

    public class OrderService : IOrderService
    {
        private readonly OnlineStoreDBContext _context;

        public OrderService(OnlineStoreDBContext context)
        {
            _context = context;
        }
    }
}
