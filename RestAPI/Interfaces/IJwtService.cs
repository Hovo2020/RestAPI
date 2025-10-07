using RestAPI.Models;
using System.Security.Claims;

namespace RestAPI.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(UserDto user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        string GetTokenIdFromToken(string token);
    }
}
