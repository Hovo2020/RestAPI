using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Interfaces;
using RestAPI.Models;
using RestAPI.Services;
using System.Security.Claims;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and get JWT tokens
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail("Invalid request data"));
                }

                var user = await _userService.AuthenticateAsync(request.Email, request.Password);
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

                var accessTokenExpires = DateTime.UtcNow.AddMinutes(15);
                var refreshTokenExpires = DateTime.UtcNow.AddDays(7);

                _logger.LogInformation("User logged in successfully: {Email}", request.Email);

                return Ok(AuthResponse<UserDto>.Success(
                    user,
                    accessToken,
                    refreshToken.Token,
                    accessTokenExpires,
                    refreshTokenExpires,
                    "Login successful"
                ));
            }
            catch (Exceptions.NotFoundException)
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid email or password"));
            }
            catch (Exceptions.BusinessRuleException ex)
            {
                return Unauthorized(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred during login"));
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail("Invalid request data"));
                }

                var userDto = await _userService.CreateUserAsync(request);
                var accessToken = _jwtService.GenerateAccessToken(userDto);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userDto.Id);

                var accessTokenExpires = DateTime.UtcNow.AddMinutes(15);
                var refreshTokenExpires = DateTime.UtcNow.AddDays(7);

                _logger.LogInformation("User registered successfully: {Email}", request.Email);

                return Ok(AuthResponse<UserDto>.Success(
                    userDto,
                    accessToken,
                    refreshToken.Token,
                    accessTokenExpires,
                    refreshTokenExpires,
                    "Registration successful"
                ));
            }
            catch (Exceptions.ConflictException ex)
            {
                return Conflict(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exceptions.BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Email}", request.Email);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred during registration"));
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] TokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.Fail("Access token and refresh token are required"));
                }

                // Validate refresh token
                var isValid = await _refreshTokenService.IsRefreshTokenValidAsync(request.RefreshToken);
                if (!isValid)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid refresh token"));
                }

                // Get user from expired access token
                var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Get user data
                var user = await _userService.GetUserByIdAsync(userId);

                // Generate new tokens
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

                // Revoke old refresh token
                await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);

                var accessTokenExpires = DateTime.UtcNow.AddMinutes(15);
                var refreshTokenExpires = DateTime.UtcNow.AddDays(7);

                _logger.LogInformation("Tokens refreshed for user: {Email}", user.Email);

                return Ok(AuthResponse<UserDto>.Success(
                    user,
                    newAccessToken,
                    newRefreshToken.Token,
                    accessTokenExpires,
                    refreshTokenExpires,
                    "Tokens refreshed successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing tokens");
                return Unauthorized(ApiResponse<object>.Fail("Token refresh failed"));
            }
        }

        /// <summary>
        /// Revoke refresh token (logout)
        /// </summary>
        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.Fail("Refresh token is required"));
                }

                await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);

                _logger.LogInformation("Refresh token revoked");

                return Ok(ApiResponse<object>.Ok(null, "Token revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return BadRequest(ApiResponse<object>.Fail("Failed to revoke token"));
            }
        }

        /// <summary>
        /// Get current user info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var user = await _userService.GetUserByIdAsync(userId);

                return Ok(ApiResponse<UserDto>.Ok(user, "Current user data"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return Unauthorized(ApiResponse<object>.Fail("Unable to get user data"));
            }
        }

        [HttpGet("start-google-login")]
        public IActionResult StartGoogleLogin()
        {
            // This triggers the server-side OAuth flow
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/google-callback"
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            // Authenticate the user
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Redirect("/?error=Google+authentication+failed");
            }

            // Get user info from claims - CORRECTED
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var givenName = result.Principal.FindFirstValue(ClaimTypes.GivenName);
            var surname = result.Principal.FindFirstValue(ClaimTypes.Surname);

            // If name is null, try to construct it
            name = name ?? $"{givenName} {surname}".Trim();

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name ?? email)
            };

            // Add additional claims if available
            var picture = result.Principal.FindFirstValue("picture");
            if (!string.IsNullOrEmpty(picture))
            {
                claims.Add(new Claim("picture", picture));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = "/"
            };

            // Sign in the user
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Redirect("/?message=Welcome+" + Uri.EscapeDataString(name ?? email) + "+!+Successfully+signed+in.");
        }

        [HttpGet("user")]
        public IActionResult GetUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Ok(new
                {
                    isAuthenticated = true,
                    email = User.FindFirstValue(ClaimTypes.Email),
                    name = User.FindFirstValue(ClaimTypes.Name)
                });
            }

            return Ok(new { isAuthenticated = false });
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/?message=Successfully+logged+out");
        }

        [HttpPost("google-token")]
        public IActionResult VerifyGoogleToken([FromBody] GoogleTokenRequest request)
        {
            try
            {
                // For now, just return success - in production, validate the token
                return Ok(new
                {
                    success = true,
                    message = "Google authentication successful",
                    user = request.User
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        public class GoogleTokenRequest
        {
            public string IdToken { get; set; }
            public GoogleUser User { get; set; }
        }

        public class GoogleUser
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Picture { get; set; }
        }
    }
}