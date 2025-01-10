using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.DataAccessLayer.Dtos;
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
        public IActionResult CreateGroup([FromBody] CreateNewGroupDto group)
        {
            _groupService.CreateNewGroupAsync(group);
            return Ok();
        }
    }
}
