﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly OnlineStoreDBContext _context;

        public OrderService(OnlineStoreDBContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<Order> GetOrderById(Guid id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public async Task AddOrder(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderById(Guid id, Order updatedOrder)
        {
            updatedOrder.Id = id;
            _context.Orders.Update(updatedOrder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderById(Guid id)
        {
            _context.Orders.Remove(new Order { Id = id });
            await _context.SaveChangesAsync();
        }
    }
}
