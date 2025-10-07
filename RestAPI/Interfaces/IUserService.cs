using RestAPI.Models;

namespace RestAPI.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request);
        Task DeleteUserAsync(string id);
        Task<List<UserDto>> GetAllUsersAsync();
    }
}
