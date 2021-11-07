using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public interface IProductService
    {
        public Task<List<Product>> GetProducts();
        public Task<Product> GetProductById(Guid id);
        public Task AddProduct(Product product);
        public Task UpdateProductById(Guid id, Product updatedProduct);
        public Task DeleteProductById(Guid id);
    }

    public class ProductService : IProductService
    {
        private readonly OnlineStoreDBContext _context;

        public ProductService(OnlineStoreDBContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetProductById(Guid id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task AddProduct(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductById(Guid id, Product updatedProduct)
        {
            updatedProduct.Id = id;
            _context.Products.Update(updatedProduct);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductById(Guid id)
        {
            _context.Products.Remove(new Product { Id = id });
            await _context.SaveChangesAsync();
        }
    }

}
