using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.enums;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Webshop.Middleware
{
    class JwtMiddleware : IFunctionsWorkerMiddleware
    {
        ILogger Logger { get; }

        public JwtMiddleware(ILogger<JwtMiddleware> Logger)
        {
            this.Logger = Logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            string headersString = (string) context.BindingContext.BindingData["Headers"];
            Dictionary<string, string> headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersString);

            if (headers.TryGetValue("Authorization", out string authValue))
            {
                IDictionary<string, object> claims;
                try
                {
                    if (authValue.StartsWith("Bearer"))
                    {
                        authValue = authValue[7..];
                    }

                    claims = new JwtBuilder()
                        .WithAlgorithm(new HMACSHA256Algorithm())
                        .WithSecret(Environment.GetEnvironmentVariable("JwtString"))
                        .MustVerifySignature()
                        .Decode<IDictionary<string, object>>(authValue);

                    UserType userType = (UserType) Convert.ToInt32(claims["userType"]);
                    Guid userId = new(Convert.ToString(claims["userId"]));

                    context.Items["UserID"] = userId;
                    context.Items["UserType"] = userType;
                }
                catch (TokenExpiredException)
                {
                    context.Items["Error"] = ErrorStatus.Expired;                  
                }
                catch (SignatureVerificationException)
                {
                    context.Items["Error"] = ErrorStatus.Invalid;
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                }
            } else {
                context.Items["Error"] = ErrorStatus.Empty;
            }

            await next(context);
        }
    }
}
