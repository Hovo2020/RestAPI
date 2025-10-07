using Microsoft.AspNetCore.Mvc;
using RestAPI.Constants;
using RestAPI.Controllers;
using RestAPI.Models;
using RestAPI.Services;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ApiControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService, ILogger<UsersController> logger)
            : base(logger)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<List<UserDto>>.Ok(users));
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return OkOrNotFound(user, ResourceNames.User);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="request">User creation data</param>
        /// <returns>Created user details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            ValidateModelState();

            var user = await _userService.CreateUserAsync(request);
            return CreatedAt(nameof(GetUser), new { id = user.Id }, user, SuccessMessages.UserCreated);
        }

        protected IActionResult CreatedAt<T>(string actionName, object routeValues, T value, string message = "")
        {
            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(value, message));
        }

        // Alternative Created method for simpler cases
        protected IActionResult Created<T>(string uri, T value, string message = "")
        {
            return Created(uri, ApiResponse<T>.Ok(value, message));
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">User update data</param>
        /// <returns>Updated user details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            ValidateModelState();

            var user = await _userService.UpdateUserAsync(id, request);
            return Ok(ApiResponse<UserDto>.Ok(user, SuccessMessages.UserUpdated));
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}