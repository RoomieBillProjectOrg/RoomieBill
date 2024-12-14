using Roomiebill.Server.BillingService;
using Roomiebill.Server.UserService;

namespace Roomiebill.Server.GroupService
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public User Admin { get; set; }
        public List<User> Members { get; set; }
        public List<Expense> Expenses { get; set; }
    }
}
