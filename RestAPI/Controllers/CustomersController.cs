using Microsoft.AspNetCore.Mvc;
using RestAPI.Constants;
using RestAPI.Controllers;
using RestAPI.Models;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CustomersController : ApiControllerBase
    {
        public CustomersController(ILogger<CustomersController> logger) : base(logger)
        {
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <param name="customer">Customer data</param>
        /// <returns>Created customer details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult CreateCustomer([FromBody] Customer customer)
        {
            ValidateModelState();

            // In a real application, you would save the customer to a database
            return Ok(ApiResponse<Customer>.Ok(customer, SuccessMessages.CustomerCreated));
        }
    }
}