﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FrontendApplication.Models
{
    public class ExpenseModel
    {
        public int Id { get; set; }
        public double Amount { get; set; } // Total amount of the expense
        public string Description { get; set; } = string.Empty; // Description of the expense
        public bool IsPaid { get; set; } = false; // Status of the expense (paid or not)
        public int PayerId { get; set; } // Foreign Key to the User who paid
        public int GroupId { get; set; } // Foreign Key to the Group this expense belongs to
        public ICollection<ExpenseSplitModel> ExpenseSplits { get; set; } = new List<ExpenseSplitModel>(); // Navigation property for ExpenseSplit
        public Category Category { get; set; } = Category.Other; // Category of the expense
        public DateTime? StartMonth { get; set; } // Start month for recurring expenses
        public DateTime? EndMonth { get; set; } // End month for recurring expenses
        public bool HasMonths => Category != Category.Other && StartMonth.HasValue && EndMonth.HasValue; // For XAML binding
    }
}
