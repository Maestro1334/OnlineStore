using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Domain;
using Domain.enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Webshop.DTO;
using Service;

namespace Webshop
{
    public class AuthHttpTrigger
    {
        ILogger Logger { get; }
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthHttpTrigger(ILogger<AuthHttpTrigger> logger, IAuthService authService, IUserService userService)
        {
            Logger = logger;
            _authService = authService;
            _userService = userService;
        }

        [Function(nameof(AuthHttpTrigger.Login))]
        public async Task<HttpResponseData> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "login")] HttpRequestData req,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Credential credential = JsonConvert.DeserializeObject<Credential>(requestBody);

            if (credential == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.WriteString("JSON object needs to be of User object format");
                return response;
            }

            User user = _authService.GetUserByUsernamePassword(credential.Username, credential.Password);

            if (user == null)
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.WriteString("Your username and password don't match.");
                return response;
            }

            AuthToken authToken = await _authService.CreateAuthToken(user);
            await response.WriteAsJsonAsync(authToken);
            return response;
        }

        [Function(nameof(AuthHttpTrigger.Refresh))]
        public async Task<HttpResponseData> Refresh(
        [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "refresh")] HttpRequestData req,
        FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            if (!_authService.VerifyRefreshToken(req, out Guid userId, out ErrorStatus errorStatus, out string refreshToken))
            {
                if (errorStatus == ErrorStatus.Invalid)
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.WriteString("Your refresh token is invalid. Please login again.");
                    return response;
                }

                if (errorStatus == ErrorStatus.Expired)
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.WriteString("Your refresh token has expired. Please login again.");
                    return response;
                }

                if (errorStatus == ErrorStatus.Empty)
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.WriteString("This request requires a bearer refresh token in the Authorizor header.");
                    return response;
                }

                response.StatusCode = HttpStatusCode.InternalServerError;
                response.WriteString("Unexpected error occured during token validation, please try again.");
                return response;
            }

            if (await _authService.GetRefreshTokenByToken(refreshToken) == null)
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.WriteString("Your refresh token is invalid. Please login again.");
                return response;
            }

            User user = await _userService.GetUserById(userId);
            if (user == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.WriteString("Your account was not found, please login again.");
                return response;
            }

            AuthToken authToken = await _authService.CreateAuthToken(user);
            await response.WriteAsJsonAsync(authToken);
            return response;
        }

        [Function(nameof(AuthHttpTrigger.Logout))]
        public async Task<HttpResponseData> Logout(
        [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "logout")] HttpRequestData req,
        FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            if(!_authService.GetTokenFromHeader(req, out string token))
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.WriteString("This request requires a bearer refresh token in the Authorizor header.");
                return response;
            }

            try
            {
                RefreshToken removalToken = await _authService.GetRefreshTokenByToken(token);
                if (removalToken == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("This refresh token has already been invalidated.");
                    return response;
                }
                
                await _authService.RemoveRefreshToken(removalToken);
            } 
            catch
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.WriteString("Unexpected error occured during token removal, please try again later.");
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            response.WriteString("You have been logged out, the refresh token has been invalidated.");
            return response;
        }
    }
}
