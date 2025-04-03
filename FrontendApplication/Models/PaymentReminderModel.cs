namespace FrontendApplication.Models
{
    public class PaymentReminderModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public Category Category { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public int DayOfMonth { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastReminderSent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentReminderRequest
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public Category Category { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public int DayOfMonth { get; set; }
    }

    public class UpdatePaymentReminderRequest
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public int DayOfMonth { get; set; }
        public bool IsActive { get; set; }
    }
}
