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
using Service.Interfaces;
using Service.Validators;
using Webshop.Security;

namespace Webshop.Controllers
{
    public class OrderHttpTrigger : AuthBase
    {
        private readonly IOrderService _orderService;

        public OrderHttpTrigger(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Function(nameof(OrderHttpTrigger.GetAllOrdersAsync))]
        public async Task<HttpResponseData> GetAllOrdersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "orders")] HttpRequestData req,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                try
                {
                    List<Order> orders = await _orderService.GetOrders();
                    await response.WriteAsJsonAsync(orders);
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

        [Function(nameof(OrderHttpTrigger.GetOrderByIdAsync))]
        public async Task<HttpResponseData> GetOrderByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "orders/{orderId}")] HttpRequestData req,
            string orderId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(orderId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    Order order = await _orderService.GetOrderById(id);
                    if (order == null)
                    {
                        response.StatusCode = HttpStatusCode.NotFound;
                        return response;
                    }

                    await response.WriteAsJsonAsync(order);
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

        [Function(nameof(OrderHttpTrigger.AddOrderAsync))]
        public async Task<HttpResponseData> AddOrderAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "orders")] HttpRequestData req,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Order order = JsonConvert.DeserializeObject<Order>(requestBody);

            if (order == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.WriteString("JSON object needs to be of Order object format");
                return response;
            }

            try
            {
                await _orderService.AddOrder(order);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                response.StatusCode = ex.StatusCode;
                return response;
            }

            response.StatusCode = HttpStatusCode.Created;
            return response;
        }

        [Function(nameof(OrderHttpTrigger.UpdateOrderByIdAsync))]
        public async Task<HttpResponseData> UpdateOrderByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "orders/{orderId}")] HttpRequestData req,
            string orderId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(orderId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    Order order = JsonConvert.DeserializeObject<Order>(requestBody);

                    if (order == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.WriteString("JSON object needs to be of Order object format");
                        return response;
                    }

                    await _orderService.UpdateOrderById(id, order);
                    await response.WriteAsJsonAsync(order);
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

        [Function(nameof(OrderHttpTrigger.DeleteOrderByIdAsync))]
        public async Task<HttpResponseData> DeleteOrderByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "orders/{orderId}")] HttpRequestData req,
            string orderId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(orderId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    await _orderService.DeleteOrderById(id);
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
