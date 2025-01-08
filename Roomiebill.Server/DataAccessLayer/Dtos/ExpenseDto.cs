using System.Collections.Generic;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class ExpenseDto
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public required string Description { get; set; }
        public bool IsPaid { get; set; }
        public int PayerId { get; set; }
        public required Dictionary<int, double> SplitBetween { get; set; } // user  and percentage
    }
}