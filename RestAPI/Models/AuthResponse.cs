namespace RestAPI.Models
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpires { get; set; }
        public DateTime RefreshTokenExpires { get; set; }
        public UserDto User { get; set; } = new UserDto();
    }

    public class AuthResponse<T> : ApiResponse<T>
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpires { get; set; }
        public DateTime RefreshTokenExpires { get; set; }

        public static AuthResponse<T> Success(T data, string accessToken, string refreshToken, DateTime accessTokenExpires, DateTime refreshTokenExpires, string message = null)
        {
            return new AuthResponse<T>
            {
                //Success = true,
                Data = data,
                Message = message,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpires = accessTokenExpires,
                RefreshTokenExpires = refreshTokenExpires
            };
        }
    }
}