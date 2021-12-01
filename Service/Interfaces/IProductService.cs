using Domain;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IProductService
    {
        public Task<List<Product>> GetProducts();
        public Task<Product> GetProductById(Guid id);
        public Task AddProduct(Product product);
        public Task UpdateProductById(Guid id, Product updatedProduct);
        public Task DeleteProductById(Guid id);
        public Task<string> AddImageToProduct(FilePart image);
    }
}
