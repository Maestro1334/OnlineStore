using DAL;
using Domain;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using JWT.Algorithms;
using JWT;
using JWT.Serializers;
using System.Linq;
using System.Collections.Generic;
using Domain.enums;
using JWT.Builder;
using JWT.Exceptions;

namespace Service
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

    public class AuthService : IAuthService
    {
        private readonly OnlineStoreDBContext _context;
        private readonly IJwtAlgorithm _algorithm;
        private readonly IJsonSerializer _serializer;
        private readonly IBase64UrlEncoder _base64Encoder;
        private readonly IJwtEncoder _jwtEncoder;

        public AuthService(OnlineStoreDBContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
            _algorithm = new HMACSHA256Algorithm();
            _serializer = new JsonNetSerializer();
            _base64Encoder = new JwtBase64UrlEncoder();
            _jwtEncoder = new JwtEncoder(_algorithm, _serializer, _base64Encoder);
        }

        public User GetUserByUsernamePassword(string username, string password)
        {
            return _context.Users.Where(u => u.Username == username && u.Password == password).FirstOrDefault();
        }

        public async Task<RefreshToken> GetRefreshTokenByToken(string token)
        {
            return await _context.Tokens.Where(t => t.Token == token).FirstOrDefaultAsync();
        }

        public async Task<RefreshToken> GetRefreshTokenByUserId(Guid userId)
        {
            return await _context.Tokens.Where(t => t.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task AddRefreshToken(RefreshToken token)
        {
            await _context.Tokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRefreshToken(RefreshToken token)
        {
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
        }

        public async Task<AuthToken> CreateAuthToken(User user)
        {
            long expiresAt = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();

            Dictionary<string, object> claims = new()
            {
                { "userId", user.Id },
                { "username", user.Username },
                { "userType", user.UserType },
                { "nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", expiresAt }
            };
            string token = _jwtEncoder.Encode(claims, Environment.GetEnvironmentVariable("JwtString"));

            Dictionary<string, object> refreshClaims = new()
            {
                { "userId", user.Id },
                { "username", user.Username },
                { "userType", user.UserType },
                { "nbf", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "exp", DateTimeOffset.UtcNow.AddHours(4).ToUnixTimeSeconds() }
            };
            string refreshToken = _jwtEncoder.Encode(refreshClaims, Environment.GetEnvironmentVariable("RefreshTokenString"));

            RefreshToken removalToken = await GetRefreshTokenByUserId(user.Id);
            if (removalToken != null)
            {
                await RemoveRefreshToken(removalToken);
            }
            await AddRefreshToken(new RefreshToken()
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedAt = DateTime.Now
            });

            return new AuthToken() { BearerToken = token, RefreshToken = refreshToken, ExpiresAt = expiresAt };
        }

        public bool VerifyRefreshToken(HttpRequestData request, out Guid userId, out ErrorStatus errorSatus, out string refreshToken)
        {
            userId = Guid.Empty;
            errorSatus = default;

            if (!GetTokenFromHeader(request, out refreshToken))
            {
                errorSatus = ErrorStatus.Empty;
                return false;
            }

            IDictionary<string, object> claims;
            try
            {
                claims = new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(Environment.GetEnvironmentVariable("RefreshTokenString"))
                    .MustVerifySignature()
                    .Decode<IDictionary<string, object>>(refreshToken);

                userId = new(Convert.ToString(claims["userId"]));
            }
            catch (TokenExpiredException)
            {
                errorSatus = ErrorStatus.Expired;
                return false;
            }
            catch (SignatureVerificationException)
            {
                errorSatus = ErrorStatus.Invalid;
                return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool GetTokenFromHeader(HttpRequestData request, out string token)
        {
            token = string.Empty;
            IEnumerable<string> authvalueList = Enumerable.Empty<string>();

            request.Headers.TryGetValues("Authorization", out authvalueList);
            if (authvalueList == null || !authvalueList.Any())
            {
                return false;
            }

            string authValue = authvalueList.FirstOrDefault();
            if (string.IsNullOrEmpty(authValue))
            {
                return false;
            }

            if (authValue.StartsWith("Bearer"))
            {
                authValue = authValue[7..];
            }

            token = authValue;
            return true;
        }
    }
}
