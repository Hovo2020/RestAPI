using RestAPI.Interfaces;
using RestAPI.Models;

namespace RestAPI.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(IDatabaseService databaseService, IJwtService jwtService, ILogger<RefreshTokenService> logger)
        {
            _databaseService = databaseService;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = _jwtService.GenerateRefreshToken(),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            var sql = @"
                INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, CreatedAt)
                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt)";

            await _databaseService.ExecuteAsync(sql, refreshToken);

            _logger.LogInformation("Generated refresh token for user {UserId}", userId);
            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            var sql = "SELECT * FROM RefreshTokens WHERE Token = @Token AND IsRevoked = 0";
            return await _databaseService.QuerySingleAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var sql = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";
            await _databaseService.ExecuteAsync(sql, new { Token = token });
            _logger.LogInformation("Revoked refresh token");
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var sql = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE UserId = @UserId";
            await _databaseService.ExecuteAsync(sql, new { UserId = userId });
            _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
        }

        public async Task<bool> IsRefreshTokenValidAsync(string token)
        {
            var refreshToken = await GetRefreshTokenAsync(token);
            return refreshToken != null &&
                   !refreshToken.IsRevoked &&
                   refreshToken.ExpiresAt > DateTime.UtcNow;
        }
    }
}