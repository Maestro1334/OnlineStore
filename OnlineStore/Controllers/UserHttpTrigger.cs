using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Domain;
using Domain.Enum;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Validators;
using Webshop.Security;
using Service;

namespace OnlineStore.Controllers
{
    public class UserHttpTrigger : AuthBase
    {
        ILogger Logger { get; }
        private readonly IUserService _userService;

        public UserHttpTrigger(ILogger<UserHttpTrigger> logger, IUserService userService)
        {
            this.Logger = logger;
            _userService = userService;
        }

        [Function(nameof(UserHttpTrigger.GetAllUsersAsync))]
        public async Task<HttpResponseData> GetAllUsersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "users")] HttpRequestData req,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                try
                {
                    List<User> users = await _userService.GetUsers();
                    await response.WriteAsJsonAsync(users);
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

        [Function(nameof(UserHttpTrigger.GetUserByIdAsync))]
        public async Task<HttpResponseData> GetUserByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "users/{userId}")] HttpRequestData req,
            string userId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(userId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    User user = await _userService.GetUserById(id);
                    if (user == null)
                    {
                        response.StatusCode = HttpStatusCode.NotFound;
                        return response;
                    }

                    await response.WriteAsJsonAsync(user);
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

        [Function(nameof(UserHttpTrigger.AddUserAsync))]
        public async Task<HttpResponseData> AddUserAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "users")] HttpRequestData req,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            User user = JsonConvert.DeserializeObject<User>(requestBody);

            if (user == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.WriteString("JSON object needs to be of User object format");
                return response;
            }

            try
            {
                await _userService.AddUser(user);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                response.StatusCode = ex.StatusCode;
                return response;
            }

            response.StatusCode = HttpStatusCode.Created;
            return response;
        }

        [Function(nameof(UserHttpTrigger.UpdateUserByIdAsync))]
        public async Task<HttpResponseData> UpdateUserByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "users/{userId}")] HttpRequestData req,
            string userId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(userId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    User user = JsonConvert.DeserializeObject<User>(requestBody);

                    if (user == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.WriteString("JSON object needs to be of User object format");
                        return response;
                    }

                    await _userService.UpdateUserById(id, user);
                    await response.WriteAsJsonAsync(user);
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

        [Function(nameof(UserHttpTrigger.DeleteUserByIdAsync))]
        public async Task<HttpResponseData> DeleteUserByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "users/{userId}")] HttpRequestData req,
            string userId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(userId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    await _userService.DeleteUserById(id);
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
