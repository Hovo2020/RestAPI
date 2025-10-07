using RestAPI.Models;

namespace RestAPI.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);
        Task DeleteUserAsync(int id);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> AuthenticateAsync(string email, string password);
        Task<UserDto> FindOrCreateOAuthUser(string email, string name, string provider);
    }
}
