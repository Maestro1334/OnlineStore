using Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IOrderService
    {
        public Task<List<Order>> GetOrders();
        public Task<Order> GetOrderById(Guid id);
        public Task AddOrder(Order order);
        public Task UpdateOrderById(Guid id, Order updatedOrder);
        public Task DeleteOrderById(Guid id);
    }
}
