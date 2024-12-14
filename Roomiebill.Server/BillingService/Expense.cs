using Roomiebill.Server.UserService;

namespace Roomiebill.Server.BillingService
{
    public class Expense
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public bool IsPaid { get; set; }
        public User Payer { get; set; }
        public List<User> SplitBetween { get; set; }
    }
}
