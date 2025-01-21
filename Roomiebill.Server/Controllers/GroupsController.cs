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
        private readonly GroupInviteMediatorService _groupInviteMediatorService;

        public GroupsController(GroupService groupService, GroupInviteMediatorService groupInviteMediatorService)
        {
            _groupService = groupService;
            _groupInviteMediatorService = groupInviteMediatorService;
        }

        [HttpPost("createNewGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateNewGroupDto group)
        {
            try
            {
                var newGroup = await _groupInviteMediatorService.CreateNewGroupSendInvitesAsync(group);
                return Ok(newGroup);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("getUserGroups")]
        public async Task<IActionResult> GetUserGroups([FromQuery] int UserId)
        {
            try
            {
                List<Group> UserGroups = await _groupService.GetUserGroupsAsync(UserId);
                return Ok(UserGroups);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("getGroup")]
        public async Task<IActionResult> GetGroup([FromQuery] int id)
        {
            try
            {
                Group group = await _groupService.GetGroupAsync(id);
                return Ok(group);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        //get all debts for a user
        [HttpGet("getDebtsForUser")]
        public async Task<IActionResult> GetDebtsForUser([FromQuery] int groupId, [FromQuery] int userId)
        {
            try
            {
                List<DebtDto> debts = await _groupService.GetDebtsForUserAsync(groupId, userId);
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("getDebtsOwedByUser")]
        public async Task<IActionResult> GetDebtsOwedByUser([FromQuery] int groupId, [FromQuery] int userId)
        {
            try
            {
                List<DebtDto> debts = await _groupService.GetDebtsOwedByUserAsync(groupId, userId);
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }

}
