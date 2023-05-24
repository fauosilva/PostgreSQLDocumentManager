using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using ApplicationCore.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PostgreSQLDocumentManager.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion(1.0)]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> logger;
        private readonly IUserService userService;

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            this.logger = logger;
            this.userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponse>), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {

            using (logger.BeginScope(new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("TransactionId", "sample"),
            }))
            {
                var user = await userService.GetUsersAsync(cancellationToken);               
                return Ok(user);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<UserResponse>), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
        {
            var user = await userService.GetUserAsync(id, cancellationToken);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateUserResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(CreateUserRequest createUserRequest, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdUser = await userService.CreateUserAsync(createUserRequest, cancellationToken);
            return Ok(createdUser);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest updateUserRequest, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdUser = await userService.UpdateUserAsync(id, updateUserRequest, cancellationToken);
            if (createdUser == null)
            {
                logger.LogInformation("Attempt to update non-existing user id: {id}.", id);
                return NotFound();
            }

            return Ok(createdUser);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            var success = await userService.DeleteUserAsync(id, cancellationToken);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
