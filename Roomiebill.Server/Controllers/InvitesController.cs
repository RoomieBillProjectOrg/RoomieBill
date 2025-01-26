using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Services;

namespace Roomiebill.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvitesController : ControllerBase
    {
        private readonly InviteService _inviteService;

        public InvitesController(InviteService inviteService)
        {
            _inviteService = inviteService;
        }

        [HttpPost("answerInvite")]
        public async Task<IActionResult> AnswerInvite([FromBody] AnswerInviteByUserDto inviteAnswer)
        {
            try
            {
                await _inviteService.AnswerInviteByUser(inviteAnswer);
                return Ok(new { Message = "Invite accepted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("inviteUserToGroupByUsername")]
        public async Task<IActionResult> InviteToGroupByUsername([FromBody] InviteToGroupByUsernameDto inviteDetails)
        {
            try
            {
                await _inviteService.InviteToGroupByUsername(inviteDetails);
                return Ok(new { Message = "Invite sent successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}