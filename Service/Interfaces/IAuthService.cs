using Domain;
using Domain.Enum;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IAuthService
    {
        public User GetUserByUsernamePassword(string username, string password);
        public Task<RefreshToken> GetRefreshTokenByToken(string token);
        public Task<RefreshToken> GetRefreshTokenByUserId(Guid userId);
        public Task AddRefreshToken(RefreshToken token);
        public Task RemoveRefreshToken(RefreshToken token);
        public Task<AuthToken> CreateAuthToken(User user);
        public bool VerifyRefreshToken(HttpRequestData request, out Guid userId, out ErrorStatus errorStatus, out string refreshToken);
        public bool GetTokenFromHeader(HttpRequestData request, out string token);
    }
}
