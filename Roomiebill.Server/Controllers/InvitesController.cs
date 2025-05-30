using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Roomiebill.Server.Services.Interfaces;

namespace Roomiebill.Server.Controllers
{
    /// <summary>
    /// Handles group invitation operations including sending and responding to invites.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InvitesController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InvitesController(IInviteService inviteService)
        {
            _inviteService = inviteService;
        }

        /// <summary>
        /// Processes a user's response to a group invitation.
        /// </summary>
        /// <param name="inviteAnswer">The user's response to the invitation.</param>
        /// <returns>Success message if the invite was processed successfully.</returns>
        /// <response code="200">When the invite is processed successfully.</response>
        /// <response code="400">If there's an error processing the invite.</response>
        [HttpPost("answerInvite")]
        public async Task<IActionResult> AnswerInvite([FromBody] AnswerInviteByUserDto inviteAnswer)
        {
            if (inviteAnswer == null)
            {
                return BadRequest(new MessageResponse { Message = "Invalid request: Input cannot be null" });
            }

            try
            {
                await _inviteService.AnswerInviteByUser(inviteAnswer);
                return Ok(new MessageResponse { Message = "Invite accepted successfully" });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message.ToLower();
                if (errorMessage.Contains("unexpected error"))
                {
                    return BadRequest(new MessageResponse { Message = "Unexpected error" });
                }
                return BadRequest(new MessageResponse { Message = "Failed to process invite response" });
            }
        }

        /// <summary>
        /// Sends a group invitation to a user via email.
        /// </summary>
        /// <param name="inviteDetails">Details of the invitation including recipient email and group info.</param>
        /// <returns>Success message if the invite was sent successfully.</returns>
        /// <response code="200">When the invite is sent successfully.</response>
        /// <response code="400">If there's an error sending the invite.</response>
        [HttpPost("inviteUserToGroupByEmail")]
        public async Task<IActionResult> InviteToGroupByEmail([FromBody] InviteToGroupByEmailDto inviteDetails)
        {
            if (inviteDetails == null)
            {
                return BadRequest(new MessageResponse { Message = "Invalid request: Input cannot be null" });
            }

            if (string.IsNullOrEmpty(inviteDetails.InviterUsername))
            {
                return BadRequest(new MessageResponse { Message = "Failed to send invite: Invalid username" });
            }

            try
            {
                await _inviteService.InviteToGroupByEmail(inviteDetails);
                return Ok(new MessageResponse { Message = "Invite sent successfully" });
            }
            catch (Exception ex) 
            {
                var error = ex.Message.ToLower();
                if (error.Contains("unexpected invite error"))
                {
                    return BadRequest(new MessageResponse { Message = "Unexpected invite error" });
                }
                if (error.Contains("invalid email"))
                {
                    return BadRequest(new MessageResponse { Message = "Invalid email" });
                }
                if (error.Contains("group not found"))
                {
                    return BadRequest(new MessageResponse { Message = "Group not found" });
                }
                return BadRequest(new MessageResponse { Message = ex.Message });
            }
        }
    }
}
