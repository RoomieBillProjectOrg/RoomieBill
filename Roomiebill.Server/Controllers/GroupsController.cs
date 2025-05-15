using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Roomiebill.Server.Common;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;

namespace Roomiebill.Server.Controllers
{
    /// <summary>
    /// Handles group-related operations including management, expenses, and debts.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GroupsController : ControllerBase
    {
        private readonly GroupService _groupService;
        private readonly GroupInviteMediatorService _groupInviteMediatorService;

        public GroupsController(GroupService groupService, GroupInviteMediatorService groupInviteMediatorService, GeminiService geminiService)
        {
            _groupService = groupService;
            _groupInviteMediatorService = groupInviteMediatorService;
        }

        /// <summary>
        /// Creates a new group and sends invites to members.
        /// </summary>
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

        /// <summary>
        /// Gets all groups for a specific user.
        /// </summary>
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

        /// <summary>
        /// Gets details of a specific group.
        /// </summary>
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

        /// <summary>
        /// Gets all debts owed to a user in a group.
        /// </summary>
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

        /// <summary>
        /// Gets all debts a user owes to others in a group.
        /// </summary>
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

        /// <summary>
        /// Adds a new expense to the group.
        /// </summary>
        [HttpPost("addExpense")]
        public async Task<IActionResult> AddExpenseAsync([FromBody] ExpenseDto expenseDto)
        {
            try
            {
                await _groupService.AddExpenseAsync(expenseDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all expenses for a specific group.
        /// </summary>
        [HttpGet("getExpensesForGroup")]
        public async Task<IActionResult> GetExpensesForGroup([FromQuery] int groupId)
        {
            try
            {
                var transactions = await _groupService.GetExpensesForGroupAsync(groupId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Snoozes a payment reminder for a member.
        /// </summary>
        [HttpPost("snoozeMemberToPay")]
        public async Task<IActionResult> SnoozeMemberToPay([FromBody] SnoozeToPayDto snoozeInfo)
        {
            try
            {
                await _groupService.SnoozeMemberToPayAsync(snoozeInfo);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets AI-generated analysis of group expenses.
        /// </summary>
        [HttpGet("getGeiminiResponseForExpenses")]
        public async Task<IActionResult> GetGeiminiResponseForExpenses([FromQuery] int groupId)
        {
            try
            {
                var transactions = await _groupService.GetExpensesForGroupAsync(groupId);
                var transactionsString = JsonSerializer.Serialize(transactions, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                var prompt = $"Please provide helpful feedback based on this group expense data: {transactionsString} make it short and in dots, talk about each type of expense and compere it to an average expense in Israel. " +
                    $"Also, please provide a summary of the total expenses and any recommendations for the group members.";
                var feedback = await _groupService.GetFeedbackFromGeminiAsync(prompt);
                return Ok(feedback);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a group if the user is the admin.
        /// </summary>
        [HttpPost("deleteGroup")]
        public async Task<IActionResult> DeleteGroup([FromQuery] int groupId, [FromQuery] int userId)
        {
            try
            {
                await _groupService.DeleteGroupAsync(groupId, userId);
                return Ok(new { Message = "Group successfully deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Removes a user from a group.
        /// </summary>
        [HttpPost("exitGroup")]
        public async Task<IActionResult> ExitGroup([FromQuery] int userId, [FromQuery] int groupId)
        {
            try
            {
                await _groupService.ExitGroupAsync(userId, groupId);
                return Ok(new { Message = "Successfully left the group" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
