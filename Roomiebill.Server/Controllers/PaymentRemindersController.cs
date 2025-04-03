using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentRemindersController : ControllerBase
    {
        private readonly IApplicationDbContext _dbContext;

        public class CreateReminderRequest
        {
            public int UserId { get; set; }
            public int GroupId { get; set; }
            public Category Category { get; set; }
            public RecurrencePattern RecurrencePattern { get; set; }
            public int DayOfMonth { get; set; }
        }

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

        [HttpPost("CreateReminder")]
        public async Task<IActionResult> CreateReminder(CreateReminderRequest request)
        {
            try
            {
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

        [HttpPut("UpdateReminder/{id}")]
        public async Task<IActionResult> UpdateReminder(UpdateReminderRequest request)
        {
            try
            {
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

        [HttpGet("GetUserReminders/user/{userId}")]
        public async Task<IActionResult> GetUserReminders(int userId)
        {
            try
            {
                var reminders = await _dbContext.GetActiveRemindersAsync();
                var userReminders = reminders.Where(r => r.UserId == userId).ToList();
                return Ok(userReminders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

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
                return StatusCode(500, ex.Message);
            }
        }
    }
}
