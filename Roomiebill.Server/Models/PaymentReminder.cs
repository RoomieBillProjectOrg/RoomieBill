using System.ComponentModel.DataAnnotations;
using Roomiebill.Server.Common.Enums;

namespace Roomiebill.Server.Models
{
    public class PaymentReminder
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required]
        public Category Category { get; set; }

        [Required]
        public RecurrencePattern RecurrencePattern { get; set; }

        [Required]
        [Range(1, 28, ErrorMessage = "Day must be between 1 and 28")]
        public int DayOfMonth { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime LastReminderSent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        public PaymentReminder()
        {
            // Required by EF Core
        }

        public PaymentReminder(int userId, int groupId, Category category, RecurrencePattern recurrencePattern, int dayOfMonth)
        {
            if (dayOfMonth < 1 || dayOfMonth > 28)
                throw new ArgumentException("Day must be between 1 and 28", nameof(dayOfMonth));

            UserId = userId;
            GroupId = groupId;
            Category = category;
            RecurrencePattern = recurrencePattern;
            DayOfMonth = dayOfMonth;
            LastReminderSent = DateTime.UtcNow.AddMonths(-1); // Set to last month to allow immediate check
        }

        public bool ShouldSendReminder(DateTime? currentDate = null)
        {
            var today = (currentDate ?? DateTime.UtcNow).Date;
            var lastSent = LastReminderSent.Date;
            
            // Check if it's the right day of the month
            if (today.Day != DayOfMonth)
                return false;

            // Calculate months between dates
            var monthDifference = ((today.Year - lastSent.Year) * 12) + today.Month - lastSent.Month;
            
            // For monthly reminders, ensure exactly one month has passed
            if (RecurrencePattern == RecurrencePattern.Monthly)
                return monthDifference == 1;

            // For bi-monthly reminders, ensure exactly two months have passed
            if (RecurrencePattern == RecurrencePattern.BiMonthly)
                return monthDifference == 2;

            return false;
        }

        public void UpdateLastReminderSent(DateTime? currentDate = null)
        {
            var now = currentDate ?? DateTime.UtcNow;
            LastReminderSent = now;
            ModifiedAt = now;
        }
    }
}
