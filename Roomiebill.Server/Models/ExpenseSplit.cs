using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Roomiebill.Server.Models
{
    public class ExpenseSplit
    {
        public int Id { get; set; }

        [Required]
        public int ExpenseId { get; set; } // Foreign Key to Expense

        [ForeignKey("ExpenseId")]
        [JsonIgnore]
        public Expense? Expense { get; set; } // Navigation Property

        [Required]
        public int UserId { get; set; } // Foreign Key to User
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; } // Navigation Property

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Invalid amount.")]
        public double Amount { get; set; } // The amount owed by the user
    }
}
