
using System.ComponentModel.DataAnnotations.Schema;

namespace Roomiebill.Server.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public User Admin { get; set; }
        public List<User> Members { get; set; }
        public List<Invite> Invites { get; set; }

        [NotMapped]
        public List<Expense> Expenses { get; set; }

        public Group()
        {
            // Default constructor required by EF Core
            Members = new List<User>();

            // REMOVE FROM COMMENT WHEN REMOVE EXPENSE CLASS FROM COMMENT
            //Expenses = new List<Expense>();
        }

        public Group(string groupName, User groupAdmin, List<User> groupMembers)
        {
            this.GroupName = groupName;
            this.Admin = groupAdmin;
            this.Members = groupMembers;

            // REMOVE FROM COMMENT WHEN REMOVE EXPENSE CLASS FROM COMMENT
            //this.Expenses = new List<Expense>();
        }

        public void AddMember(User user)
        {
            Members.Add(user);
        }

        public void RemoveMember(User user)
        {
            Members.Remove(user);
        }

        public void AddExpense(Expense expense)
        {
            // REMOVE FROM COMMENT WHEN REMOVE EXPENSE CLASS FROM COMMENT
            //Expenses.Add(expense);
        }

        public void RemoveExpense(Expense expense)
        {

            // REMOVE FROM COMMENT WHEN REMOVE EXPENSE CLASS FROM COMMENT
            //Expenses.Remove(expense);
        }

        public User GetAdmin()
        {
            return Admin;
        }

        public List<User> GetMembers()
        {
            return Members;
        }

        public string GetGroupName()
        {
            return GroupName;
        }
    }
}
