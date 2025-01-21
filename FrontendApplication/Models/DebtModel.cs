
namespace FrontendApplication.Models
{
    public class DebtModel
    {
        public int OwedByUserId { get; set; }
        public required string OwedByUserName { get; set; } // Optional: For user display
        public double Amount { get; set; }
    }
}