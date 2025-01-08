
namespace Roomiebill.Server.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public bool IsPaid { get; set; }
        public User Payer { get; set; }
        public Dictionary<int, double> SplitBetween { get; set; } //<UserId, percentage> maps between user id and percentage of the pay

    }
}
