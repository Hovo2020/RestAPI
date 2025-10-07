using Microsoft.AspNetCore.Mvc;
using RestAPI.Exceptions;
using RestAPI.Models;

namespace RestAPI.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ApiControllerBase : ControllerBase
    {
        protected readonly ILogger<ApiControllerBase> _logger;

        /// <summary>
        /// Initializes a new instance of the ApiControllerBase class
        /// </summary>
        /// <param name="logger">The logger instance for logging</param>
        public ApiControllerBase(ILogger<ApiControllerBase> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns Ok result with data or throws NotFoundException if data is null
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="result">The result data to return</param>
        /// <param name="resourceName">The name of the resource being requested (optional)</param>
        /// <returns>OkObjectResult with ApiResponse if data exists</returns>
        /// <exception cref="NotFoundException">Thrown when the result is null</exception>
        protected IActionResult OkOrNotFound<T>(T result, string resourceName = null)
        {
            if (result == null)
            {
                throw new NotFoundException(resourceName ?? "Resource", 0);
            }
            return Ok(ApiResponse<T>.Ok(result));
        }

        /// <summary>
        /// Returns a Created (201) response with location header
        /// </summary>
        /// <typeparam name="T">The type of the created resource</typeparam>
        /// <param name="actionName">The name of the action that created the resource</param>
        /// <param name="routeValues">The route values for generating the URL</param>
        /// <param name="value">The created resource data</param>
        /// <returns>CreatedAtActionResult with ApiResponse</returns>
        protected IActionResult CreatedAt<T>(string actionName, object routeValues, T value)
        {
            return CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(value, "Resource created successfully"));
        }

        /// <summary>
        /// Validates the model state and throws ValidationException if invalid
        /// </summary>
        /// <exception cref="ValidationException">Thrown when the model state is invalid</exception>
        protected void ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                throw new ValidationException(errors);
            }
        }
    }
}