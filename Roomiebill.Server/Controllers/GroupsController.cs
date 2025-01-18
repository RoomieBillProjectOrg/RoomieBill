using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;

namespace Roomiebill.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly GroupService _groupService;

        public GroupsController(GroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost("createNewGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateNewGroupDto group)
        {
            try
            {
                var newGroup = await _groupService.CreateNewGroupAsync(group);
                return Ok(newGroup);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
