using System;
using RestAPI.Interfaces;
using RestAPI.Models;

namespace RestAPI.Services
{
    public class UserService(IDatabaseService databaseService, ILogger<UserService> logger, IPasswordHasher passwordHasher) : IUserService
    {
        private readonly IDatabaseService _databaseService = databaseService;
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ILogger<UserService> _logger = logger;

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            if (id == 0)
            {
                throw new Exceptions.ArgumentException("User ID cannot be empty", nameof(id));
            }

            _logger.LogInformation("Getting user by ID: {UserId}", id);

            var user = await _databaseService.QuerySingleAsync<User>("sp_GetUserById", new { Id = id });

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} was not found", id);
                throw new Exceptions.NotFoundException("User", id);
            }

            return MapToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            // Check if user already exists using stored procedure
            var emailExists = await _databaseService.ExecuteScalarAsync("sp_CheckEmailExists", new { Email = request.Email });
            if (Convert.ToInt32(emailExists) > 0)
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
                // Hash password before storing
                var hashedPassword = _passwordHasher.HashPassword(request.Password);

                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Age = request.Age,
                    Password = hashedPassword,
                    ConfirmPassword = hashedPassword,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Create user using stored procedure
                await _databaseService.ExecuteAsync("sp_CreateUser", new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Age,
                    user.Password,
                    user.ConfirmPassword,
                    user.CreatedAt,
                    user.IsActive
                });

                _logger.LogInformation("User created with ID {UserId}", user.Id);

                return MapToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating user");
                throw new Exceptions.ApiException("Failed to create user", 500, "CREATE_USER_ERROR");
            }
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            // First check if user exists
            var existingUser = await _databaseService.QuerySingleAsync<User>("sp_GetUserById", new { Id = id });
            if (existingUser == null)
            {
                throw new Exceptions.NotFoundException("User", id);
            }

            // Check for email conflict with other users using stored procedure
            var emailExists = await _databaseService.ExecuteScalarAsync("sp_CheckEmailExists", new
            {
                Email = request.Email,
                ExcludingId = id
            });

            if (Convert.ToInt32(emailExists) > 0)
            {
                throw new Exceptions.ConflictException($"Email {request.Email} is already in use by another user");
            }

            // Update user using stored procedure
            await _databaseService.ExecuteAsync("sp_UpdateUser", new
            {
                Id = id,
                Name = request.Name,
                Email = request.Email,
                Age = request.Age,
                Password = existingUser.Password, // Keep existing password or update if provided
                ConfirmPassword = existingUser.ConfirmPassword,
                IsActive = true
            });

            _logger.LogInformation("User with ID {UserId} was updated", id);

            // Return updated user
            var updatedUser = await _databaseService.QuerySingleAsync<User>("sp_GetUserById", new { Id = id });
            return MapToDto(updatedUser);
        }

        public async Task DeleteUserAsync(int id)
        {
            // First check if user exists
            var existingUser = await _databaseService.QuerySingleAsync<User>("sp_GetUserById", new { Id = id });
            if (existingUser == null)
            {
                throw new Exceptions.NotFoundException("User", id);
            }

            // Soft delete using stored procedure
            await _databaseService.ExecuteAsync("sp_DeleteUser", new { Id = id });

            _logger.LogInformation("User with ID {UserId} was deleted", id);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("Getting all users from database");

                var users = await _databaseService.QueryAsync<User>("sp_GetAllUsers");
                var userList = users.Select(MapToDto).ToList();

                _logger.LogInformation("Retrieved {UserCount} users from database", userList.Count);
                return userList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users from database");
                throw;
            }
        }

        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Authenticating user: {Email}", email);

                var user = await _databaseService.QuerySingleAsync<User>("sp_GetUserByEmail", new { Email = email });

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Authentication failed: User not found or inactive - {Email}", email);
                    throw new Exceptions.NotFoundException("User", email);
                }

                // Verify password using hasher
                if (!_passwordHasher.VerifyPassword(password, user.Password))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for user - {Email}", email);
                    throw new Exceptions.BusinessRuleException("Invalid email or password");
                }

                _logger.LogInformation("User authenticated successfully: {Email}", email);
                return MapToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user: {Email}", email);
                throw;
            }
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var user = await _databaseService.QuerySingleAsync<User>("sp_GetUserByEmail", new { Email = email });

            if (user == null)
                throw new Exceptions.NotFoundException("User", email);

            return MapToDto(user);
        }

        public async Task<UserDto> FindOrCreateOAuthUser(string email, string name, string provider)
        {
            try
            {
                // Try to find existing user
                var existingUser = await GetUserByEmailAsync(email);
                _logger.LogInformation("OAuth user found: {Email} via {Provider}", email, provider);
                return existingUser;
            }
            catch (Exceptions.NotFoundException)
            {
                // Create new user for OAuth login
                _logger.LogInformation("Creating new OAuth user: {Email} via {Provider}", email, provider);

                var createRequest = new CreateUserRequest
                {
                    Name = name,
                    Email = email,
                    Age = 25, // Default age for OAuth users
                    Password = GenerateSecureRandomPassword() // OAuth users don't need to know this
                };

                return await CreateUserAsync(createRequest);
            }
        }

        private string GenerateSecureRandomPassword()
        {
            // Generate a secure random password for OAuth users
            var randomBytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
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