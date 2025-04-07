using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class ExpenseDto
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public double Amount { get; set; }

        [StringLength(100, ErrorMessage = "Description cannot be longer than 100 characters.")]
        public string Description { get; set; } = string.Empty;

        public bool IsPaid { get; set; }
        public int PayerId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [Required]
        public List<ExpenseSplitDto> ExpenseSplits { get; set; } = new List<ExpenseSplitDto>();

        [Required]
        public Category Category { get; set; } // Category of the expense
        public DateTime? StartMonth { get; set; } // Start month for recurring expenses
        public DateTime? EndMonth { get; set; } // End month for recurring expenses

        [Required]
        public string ReceiptString { get; set; } = "";
        
    }
}