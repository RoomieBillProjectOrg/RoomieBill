
namespace Roomiebill.Server.Models
{
    public class Group
    {
        private int Id { get; set; }
        private string Name { get; set; }
        private string Description { get; set; } 
        private User Admin { get; set; }
        private List<User> Members { get; set; }
        private List<Expense> Expenses { get; set; }

        public Group(int Id, string Name,string Description, User Admin, List<User> Members, List<Expense> Expenses){
            this.Id = Id;
            this.Name = Name;
            this.Description = Description;
            this.Admin = Admin;
            this.Members = Members;
            this.Expenses = Expenses;
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
