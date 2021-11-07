using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Domain;
using Domain.Enum;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Service;
using Service.Validators;
using Webshop.Security;

namespace OnlineStore.Controllers
{
    public class ProductHttpTrigger : AuthBase
    {
        private readonly IProductService _productService;

        public ProductHttpTrigger(IProductService productService)
        {
            _productService = productService;
        }

        [Function(nameof(ProductHttpTrigger.GetAllProductsAsync))]
        public async Task<HttpResponseData> GetAllProductsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "products")] HttpRequestData req,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            try
            {
                List<Product> products = await _productService.GetProducts();
                await response.WriteAsJsonAsync(products);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                response.StatusCode = ex.StatusCode;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        [Function(nameof(ProductHttpTrigger.GetProductByIdAsync))]
        public async Task<HttpResponseData> GetProductByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "products/{productId}")] HttpRequestData req,
            string productId,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            if (!GuidValidator.VerifyGuidType(productId, out Guid id))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.WriteString("ID needs to be of GUID format.");
                return response;
            }

            try
            {
                Product product = await _productService.GetProductById(id);
                if (product == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                await response.WriteAsJsonAsync(product);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                response.StatusCode = ex.StatusCode;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        [Function(nameof(ProductHttpTrigger.AddProductAsync))]
        public async Task<HttpResponseData> AddProductAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "products")] HttpRequestData req,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Product product = JsonConvert.DeserializeObject<Product>(requestBody);

                if (product == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("JSON object needs to be of Product object format");
                    return response;
                }

                try
                {
                    await _productService.AddProduct(product);
                }
                catch (Microsoft.Azure.Cosmos.CosmosException ex)
                {
                    response.StatusCode = ex.StatusCode;
                    return response;
                }

                response.StatusCode = HttpStatusCode.Created;
                return response;
            });
        }

        [Function(nameof(ProductHttpTrigger.UpdateProductByIdAsync))]
        public async Task<HttpResponseData> UpdateProductByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "products/{productId}")] HttpRequestData req,
            string productId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(productId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    Product product = JsonConvert.DeserializeObject<Product>(requestBody);

                    if (product == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.WriteString("JSON object needs to be of Product object format");
                        return response;
                    }

                    await _productService.UpdateProductById(id, product);
                    await response.WriteAsJsonAsync(product);
                }
                catch (Microsoft.Azure.Cosmos.CosmosException ex)
                {
                    response.StatusCode = ex.StatusCode;
                    return response;
                }

                response.StatusCode = HttpStatusCode.OK;
                return response;
            });
        }

        [Function(nameof(ProductHttpTrigger.DeleteProductByIdAsync))]
        public async Task<HttpResponseData> DeleteProductByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "products/{productId}")] HttpRequestData req,
            string productId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(productId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    await _productService.DeleteProductById(id);
                }
                catch (Microsoft.Azure.Cosmos.CosmosException ex)
                {
                    response.StatusCode = ex.StatusCode;
                    return response;
                }

                response.StatusCode = HttpStatusCode.OK;
                return response;
            });
        }
    }
}
