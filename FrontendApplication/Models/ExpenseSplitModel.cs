namespace FrontendApplication.Models
{
    public class ExpenseSplitModel
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; } // Foreign Key to Expense
        public ExpenseModel? Expense { get; set; } // Navigation Property
        public int UserId { get; set; } // Foreign Key to User
        public UserModel? User { get; set; } // Navigation Property
        public double Percentage { get; set; } // The percentage owed by the user
    }
}
