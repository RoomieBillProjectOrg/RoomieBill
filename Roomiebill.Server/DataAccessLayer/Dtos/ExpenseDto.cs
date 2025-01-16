using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class ExpenseDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public double Amount { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Description cannot be longer than 100 characters.")]
        public string Description { get; set; } = string.Empty;

        public bool IsPaid { get; set; }

        [ForeignKey("Payer")]
        public int PayerId { get; set; }

        [Required]
        public List<ExpenseSplitDto> ExpenseSplits { get; set; } = new List<ExpenseSplitDto>();
        
        [Required]
        public int GroupId { get; set; }
    }
}