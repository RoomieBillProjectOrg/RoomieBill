using System.ComponentModel.DataAnnotations;
namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class ExpenseSplitDto
    {
        public int Id { get; set; } // Unique identifier for the split

        [Required]
        public int ExpenseId { get; set; } // Foreign Key to Expense

        [Required]
        public int UserId { get; set; } // Foreign Key to User

        [Required]
        public double Amount { get; set; } // The amount owed by the user
    }
}
