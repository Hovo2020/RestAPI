using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Interfaces;
using RestAPI.Models;
using RestAPI.Services;
using System;
using System.Security.Claims;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OAuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(
            IUserService userService,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService,
            ILogger<OAuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Initiate Google OAuth flow
        /// </summary>
        [HttpGet("google")]
        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback"),
                Items = { { "scheme", "Google" } }
            };

            return Challenge(properties, "Google");
        }

        /// <summary>
        /// Handle Google OAuth callback and issue JWT
        /// </summary>
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            return await HandleOAuthCallback("Google");
        }

        private async Task<IActionResult> HandleOAuthCallback(string provider)
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(provider);

                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("OAuth authentication failed for {Provider}", provider);
                    return BadRequest(ApiResponse<object>.Fail($"{provider} authentication failed"));
                }

                var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
                var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name) ??
                          authenticateResult.Principal.FindFirstValue("name") ??
                          "Unknown User";

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("OAuth provider {Provider} did not return email", provider);
                    return BadRequest(ApiResponse<object>.Fail("Email claim not provided by OAuth provider"));
                }

                _logger.LogInformation("OAuth successful for {Email} via {Provider}", email, provider);

                // Find or create user in your system
                var user = await _userService.FindOrCreateOAuthUser(email, name, provider);

                // Generate JWT tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

                var accessTokenExpires = DateTime.UtcNow.AddMinutes(15);
                var refreshTokenExpires = DateTime.UtcNow.AddDays(7);

                // Sign out the OAuth cookie authentication
                await HttpContext.SignOutAsync(provider);

                // Return JWT tokens to client
                return Ok(AuthResponse<UserDto>.Success(
                    user,
                    accessToken,
                    refreshToken.Token,
                    accessTokenExpires,
                    refreshTokenExpires,
                    $"{provider} login successful"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth callback for {Provider}", provider);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred during OAuth authentication"));
            }
        }

        /// <summary>
        /// Get available OAuth providers
        /// </summary>
        [HttpGet("providers")]
        public IActionResult GetProviders()
        {
            var providers = new[]
            {
                new { Id = "google", Name = "Google", Url = "/api/oauth/google" },
                //new { Id = "facebook", Name = "Facebook", Url = "/api/oauth/facebook" },
                //new { Id = "microsoft", Name = "Microsoft", Url = "/api/oauth/microsoft" }
            };

            return Ok(ApiResponse<object>.Ok(providers, "Available OAuth providers"));
        }
    }
}