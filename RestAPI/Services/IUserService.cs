using RestAPI.Models;
using System;

namespace RestAPI.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request);
        Task DeleteUserAsync(string id);
        Task<List<UserDto>> GetAllUsersAsync();
    }

    public class UserService : IUserService
    {
        private static readonly List<User> _users = new();
        private readonly ILogger<UserService> _logger;

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new Exceptions.ArgumentException("User ID cannot be empty", nameof(id));
            }

            await Task.Delay(100); // Simulate async operation

            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} was not found", id);
                throw new Exceptions.NotFoundException("User", id);
            }

            return MapToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            // Check if user already exists
            var existingUser = _users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            if (existingUser != null)
            {
                throw new Exceptions.ConflictException($"User with email {request.Email} already exists");
            }

            // Business rule validation
            if (request.Age < 18)
            {
                throw new Exceptions.BusinessRuleException("User must be at least 18 years old");
            }

            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Email = request.Email,
                    Age = request.Age,
                    Password = request.Password // In real app, this would be hashed
                };

                _users.Add(user);
                _logger.LogInformation("User created with ID {UserId}", user.Id);

                return MapToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating user");
                throw new Exceptions.ApiException("Failed to create user", 500, "CREATE_USER_ERROR");
            }
        }

        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            if (user == null)
            {
                throw new Exceptions.NotFoundException("User", id);
            }

            // Check for email conflict with other users
            var emailExists = _users.Any(u => u.Id != id && u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            if (emailExists)
            {
                throw new Exceptions.ConflictException($"Email {request.Email} is already in use by another user");
            }

            user.Name = request.Name;
            user.Email = request.Email;
            user.Age = request.Age;

            _logger.LogInformation("User with ID {UserId} was updated", id);

            return MapToDto(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            if (user == null)
            {
                throw new Exceptions.NotFoundException("User", id);
            }

            // Soft delete
            user.IsActive = false;
            _logger.LogInformation("User with ID {UserId} was deleted", id);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return _users.Where(u => u.IsActive).Select(MapToDto).ToList();
        }

        private UserDto MapToDto(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Age = user.Age,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}