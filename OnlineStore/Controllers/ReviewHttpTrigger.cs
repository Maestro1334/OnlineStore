using Domain;
using Domain.Enum;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Service;
using Service.Interfaces;
using Service.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Webshop.Security;

namespace OnlineStore.Controllers
{
    public class ReviewHttpTrigger : AuthBase
    {
        private readonly IReviewService _reviewService;

        public ReviewHttpTrigger(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Function(nameof(ReviewHttpTrigger.GetAllReviewsAsync))]
        public async Task<HttpResponseData> GetAllReviewsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "reviews")] HttpRequestData req,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            try
            {
                List<Review> reviews = await _reviewService.GetReviews();
                await response.WriteAsJsonAsync(reviews);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                response.StatusCode = ex.StatusCode;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        [Function(nameof(ReviewHttpTrigger.GetReviewByIdAsync))]
        public async Task<HttpResponseData> GetReviewByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "reviews/{reviewId}")] HttpRequestData req,
            string reviewId,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            if (!GuidValidator.VerifyGuidType(reviewId, out Guid id))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.WriteString("ID needs to be of GUID format.");
                return response;
            }

            try
            {
                Review review = await _reviewService.GetReviewById(id);
                if (review == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                await response.WriteAsJsonAsync(review);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                response.StatusCode = ex.StatusCode;
                return response;
            }

            response.StatusCode = HttpStatusCode.OK;
            return response;
        }

        [Function(nameof(ReviewHttpTrigger.AddReviewAsync))]
        public async Task<HttpResponseData> AddReviewAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "reviews")] HttpRequestData req,
            FunctionContext executionContext)
        {
            HttpResponseData response = req.CreateResponse();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Review review = JsonConvert.DeserializeObject<Review>(requestBody);

            if (review == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.WriteString("JSON object needs to be of Review object format");
                return response;
            }

            try
            {
                await _reviewService.AddReview(review);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ex)
            {
                response.StatusCode = ex.StatusCode;
                return response;
            }

            response.StatusCode = HttpStatusCode.Created;
            return response;
        }

        [Function(nameof(ReviewHttpTrigger.UpdateReviewByIdAsync))]
        public async Task<HttpResponseData> UpdateReviewByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "reviews/{reviewId}")] HttpRequestData req,
            string reviewId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(reviewId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    Review review = JsonConvert.DeserializeObject<Review>(requestBody);

                    if (review == null)
                    {
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.WriteString("JSON object needs to be of Review object format");
                        return response;
                    }

                    await _reviewService.UpdateReviewById(id, review);
                    await response.WriteAsJsonAsync(review);
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

        [Function(nameof(ReviewHttpTrigger.DeleteReviewByIdAsync))]
        public async Task<HttpResponseData> DeleteReviewByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "reviews/{reviewId}")] HttpRequestData req,
            string reviewId,
            FunctionContext executionContext)
        {
            return await ExecuteForAdmin(req, executionContext, async (UserType userType) =>
            {
                HttpResponseData response = req.CreateResponse();

                if (!GuidValidator.VerifyGuidType(reviewId, out Guid id))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.WriteString("ID needs to be of GUID format.");
                    return response;
                }

                try
                {
                    await _reviewService.DeleteReviewById(id);
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
