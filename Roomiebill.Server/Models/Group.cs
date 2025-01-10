
namespace Roomiebill.Server.Models
{
    public class Group
    {
        private int Id { get; set; }
        private string GroupName { get; set; }
        private User Admin { get; set; }
        private List<User> Members { get; set; }
        private List<Expense> Expenses { get; set; }

        public Group(string groupName, User groupAdmin, List<User> groupMembers){
            this.GroupName = groupName;
            this.Admin = groupAdmin;
            this.Members = groupMembers;
            this.Expenses = new List<Expense>();
        }

        public void AddMember(User user){
            Members.Add(user);
        }

        public void RemoveMember(User user){
            Members.Remove(user);
        }

        public void AddExpense(Expense expense){
            Expenses.Add(expense);
        }

        public void RemoveExpense(Expense expense){
            Expenses.Remove(expense);
        }

        


    }
}
