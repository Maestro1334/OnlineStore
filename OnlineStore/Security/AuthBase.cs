using Domain.enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Webshop.Security
{
    public class AuthBase
    {
		protected static async Task<HttpResponseData> ExecuteForUser(HttpRequestData Request, FunctionContext ExecutionContext, Func<UserType, Task<HttpResponseData>> Delegate)
		{
			return await Verify(UserType.User, Request, ExecutionContext, Delegate);
		}

		protected static async Task<HttpResponseData> ExecuteForAdmin(HttpRequestData Request, FunctionContext ExecutionContext, Func<UserType, Task<HttpResponseData>> Delegate)
		{
			return await Verify(UserType.Admin, Request, ExecutionContext, Delegate);
		}

		private static async Task<HttpResponseData> Verify(UserType validatingUserType, HttpRequestData Request, FunctionContext ExecutionContext, Func<UserType, Task<HttpResponseData>> Delegate)
		{
			try
			{
				if (ExecutionContext.Items.ContainsKey("Error"))
                {
					ErrorStatus errorStatus = (ErrorStatus)ExecutionContext.Items["Error"];
					if (errorStatus == ErrorStatus.Expired)
					{
						HttpResponseData response = Request.CreateResponse(HttpStatusCode.Unauthorized);
						response.WriteString("Your token has expired. Please login again, or refresh your token.");
						return response;
					}

					if (errorStatus == ErrorStatus.Invalid)
					{
						HttpResponseData response = Request.CreateResponse(HttpStatusCode.Unauthorized);
						response.WriteString("Your token is invalid. Please login again, or refresh your token.");
						return response;
					}

					if (errorStatus == ErrorStatus.Empty)
                    {
						HttpResponseData response = Request.CreateResponse(HttpStatusCode.Unauthorized);
						response.WriteString("This request requires a bearer token in the Authorizor header.");
						return response;
					}
                }

				UserType userType = (UserType) ExecutionContext.Items["UserType"];

				if (validatingUserType != UserType.Admin)
				{
					if (userType != UserType.Admin)
					{
						HttpResponseData response = Request.CreateResponse(HttpStatusCode.Forbidden);
						response.WriteString("You don't have the permissions to use this function. Only admins can use this function.");
						return response;
					}
				}
				else if (userType == UserType.User)
				{
					if (userType != UserType.User && userType != UserType.Admin)
					{
						HttpResponseData response = Request.CreateResponse(HttpStatusCode.Forbidden);
						response.WriteString("You don't have the permissions to use this function. Only registered users can use this function.");
						return response;
					}
				}

				try
				{
					return await Delegate(userType).ConfigureAwait(false);
				}
				catch
				{
					HttpResponseData Response = Request.CreateResponse(HttpStatusCode.InternalServerError);
					return Response;
				}
			}
			catch
			{
				HttpResponseData response = Request.CreateResponse(HttpStatusCode.Unauthorized);
				response.WriteString("You are not authorized. Please login and add the bearer token to your request header.");
				return response;
			}
		}
	}
}
