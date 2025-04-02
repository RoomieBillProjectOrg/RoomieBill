﻿namespace FrontendApplication.Models
{
    public class ExpenseSplitModel
    {
        public int Id { get; set; }
        public int ExpenseId { get; set; } // Foreign Key to Expense
        public int UserId { get; set; } // Foreign Key to User
        public double Amount { get; set; } // The amount owed by the user
    }
}
