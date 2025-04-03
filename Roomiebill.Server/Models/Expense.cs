﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Roomiebill.Server.Common.Enums;

namespace Roomiebill.Server.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public double Amount { get; set; } // Total amount of the expense

        [Required]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string Description { get; set; } = string.Empty; // Description of the expense

        public bool IsPaid { get; set; } = false; // Status of the expense (paid or not)

        [Required]
        public int PayerId { get; set; } // Foreign Key to the User who paid

        [JsonIgnore]
        [ForeignKey("PayerId")]
        public User? Payer { get; set; } // Navigation property for the payer

        [Required]
        public int GroupId { get; set; } // Foreign Key to the Group this expense belongs to

        [JsonIgnore]
        [ForeignKey("GroupId")]
        public Group? Group { get; set; } // Navigation property for the group

        // Navigation property for ExpenseSplit
        public ICollection<ExpenseSplit> ExpenseSplits { get; set; } = new List<ExpenseSplit>();

        [Required]
        public Category Category { get; set; } // Category of the expense

        // StartMonth and EndMonth for recurring expenses (nullable for Category.Other)
        public DateTime? StartMonth { get; set; }

        public DateTime? EndMonth { get; set; }
    }
}
