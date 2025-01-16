using System.ComponentModel.DataAnnotations;

namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class ExpenseSplitDto
    {
        [Key]
        public int Id { get; set; } // Unique identifier for the split

        [Required]
        public int ExpenseId { get; set; } // Foreign Key to Expense

        [Required]
        public int UserId { get; set; } // Foreign Key to User

        [Required]
        [Range(0.01, 100.0, ErrorMessage = "Percentage must be between 0.01 and 100.")]
        public double Percentage { get; set; } // The percentage owed by the user
    }
}