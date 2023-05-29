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
    public class GroupsController : ControllerBase
    {
        private readonly ILogger<UsersController> logger;
        private readonly IGroupService groupService;

        public GroupsController(ILogger<UsersController> logger, IGroupService groupService)
        {
            this.logger = logger;
            this.groupService = groupService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponse>), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGroups(CancellationToken cancellationToken)
        {
            var groups = await groupService.GetGroupsAsync(cancellationToken);
            return Ok(groups);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GroupResponse), 200)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGroup(int id, CancellationToken cancellationToken)
        {
            var group = await groupService.GetGroupAsync(id, cancellationToken);

            if (group == null)
                return NotFound();

            return Ok(group);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateGroupResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGroup(CreateGroupRequest createGroupRequest, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdGroup = await groupService.CreateGroupAsync(createGroupRequest.Name, cancellationToken);
            return Ok(createdGroup);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGroup(int id, UpdateGroupRequest updateGroupRequest, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdGroup = await groupService.UpdateGroupAsync(id, updateGroupRequest.Name, cancellationToken);
            if (createdGroup == null)
            {
                logger.LogInformation("Attempt to update non-existing user id: {id}.", id);
                return NotFound();
            }

            return Ok(createdGroup);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(void), 401)]
        [ProducesResponseType(typeof(void), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGroup(int id, CancellationToken cancellationToken)
        {
            var success = await groupService.DeleteGroupAsync(id, cancellationToken);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
