using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Controllers
{
    /// <summary>
    /// Manages recurring payment reminders for group expenses.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentRemindersController : ControllerBase
    {
        private readonly IApplicationDbContext _dbContext;

        /// <summary>
        /// Request model for creating a new payment reminder.
        /// </summary>
        public class CreateReminderRequest
        {
            public int UserId { get; set; }
            public int GroupId { get; set; }
            public Category Category { get; set; }
            public RecurrencePattern RecurrencePattern { get; set; }
            public int DayOfMonth { get; set; }
        }

        /// <summary>
        /// Request model for updating an existing payment reminder.
        /// </summary>
        public class UpdateReminderRequest
        {
            public int Id { get; set; }
            public Category Category { get; set; }
            public RecurrencePattern RecurrencePattern { get; set; }
            public int DayOfMonth { get; set; }
            public bool IsActive { get; set; }
        }

        public PaymentRemindersController(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Creates a new payment reminder for a user in a group.
        /// </summary>
        /// <param name="request">Details for creating the reminder.</param>
        /// <returns>The created reminder.</returns>
        /// <response code="200">Returns the created reminder.</response>
        /// <response code="400">If the request is invalid or user is not in group.</response>
        /// <response code="404">If the user or group is not found.</response>
        [HttpPost("CreateReminder")]
        public async Task<IActionResult> CreateReminder(CreateReminderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid request");
                }

                if (request.DayOfMonth < 1 || request.DayOfMonth > 28)
                {
                    return BadRequest("Day of month must be between 1 and 28");
                }

                var user = await _dbContext.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var group = await _dbContext.GetGroupByIdAsync(request.GroupId);
                if (group == null)
                {
                    return NotFound("Group not found");
                }

                // Verify user is member of the group
                if (!group.Members.Any(m => m.Id == request.UserId))
                {
                    return BadRequest("User is not a member of the specified group");
                }

                var reminder = new PaymentReminder(
                    request.UserId,
                    request.GroupId,
                    request.Category,
                    request.RecurrencePattern,
                    request.DayOfMonth
                );

                await _dbContext.AddPaymentReminderAsync(reminder);

                return Ok(reminder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing payment reminder.
        /// </summary>
        /// <param name="request">Details for updating the reminder.</param>
        /// <returns>The updated reminder.</returns>
        /// <response code="200">Returns the updated reminder.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="404">If the reminder is not found.</response>
        [HttpPut("UpdateReminder/{id}")]
        public async Task<IActionResult> UpdateReminder(UpdateReminderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid request");
                }

                if (request.DayOfMonth < 1 || request.DayOfMonth > 28)
                {
                    return BadRequest("Day of month must be between 1 and 28");
                }

                var reminder = await _dbContext.GetPaymentReminderByIdAsync(request.Id);
                if (reminder == null)
                {
                    return NotFound("Reminder not found");
                }

                reminder.Category = request.Category;
                reminder.RecurrencePattern = request.RecurrencePattern;
                reminder.DayOfMonth = request.DayOfMonth;
                reminder.IsActive = request.IsActive;
                reminder.ModifiedAt = DateTime.UtcNow;

                await _dbContext.UpdatePaymentReminderAsync(reminder);

                return Ok(reminder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all active payment reminders for a user.
        /// </summary>
        /// <param name="userId">ID of the user.</param>
        /// <returns>List of active reminders for the user.</returns>
        /// <response code="200">Returns the list of reminders.</response>
        /// <response code="500">If there's a server error.</response>
        [HttpGet("GetUserReminders/user/{userId}")]
        public async Task<IActionResult> GetUserReminders(int userId)
        {
            try
            {
                var reminders = await _dbContext.GetActiveRemindersAsync();
                var userReminders = reminders.Where(r => r.UserId == userId).ToList();
                return Ok(userReminders);
            }
            catch (Exception)
            {
                var emptyList = new List<PaymentReminder>();
                return BadRequest(emptyList);
            }
        }

        /// <summary>
        /// Soft-deletes a payment reminder by marking it as inactive.
        /// </summary>
        /// <param name="id">ID of the reminder to delete.</param>
        /// <returns>Success status.</returns>
        /// <response code="200">If the reminder was successfully deactivated.</response>
        /// <response code="404">If the reminder is not found.</response>
        /// <response code="500">If there's a server error.</response>
        [HttpDelete("DeleteReminder/{id}")]
        public async Task<IActionResult> DeleteReminder(int id)
        {
            try
            {
                var reminder = await _dbContext.GetPaymentReminderByIdAsync(id);
                if (reminder == null)
                {
                    return NotFound("Reminder not found");
                }

                // Soft delete by setting IsActive to false
                reminder.IsActive = false;
                reminder.ModifiedAt = DateTime.UtcNow;

                await _dbContext.UpdatePaymentReminderAsync(reminder);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
