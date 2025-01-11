
using System.ComponentModel.DataAnnotations.Schema;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;

namespace Roomiebill.Server.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public User Admin { get; set; }
        public List<User> Members { get; set; }

        [NotMapped]
        public Dictionary<int, List<Expense>> Expenses { get; set; } // <User id , list of expenses>
        private int[] _debtArray; // 1D array to store debts
        ExpenseHandler expenseHandler { get; set; }


        public Group()
        {
            // Default constructor required by EF Core
            Members = new List<User>();
            this.GroupName = "Default Name";
            this._debtArray = new int[1];
            Expenses = new Dictionary<int, List<Expense>>();
            expenseHandler = new ExpenseHandler(Members);
        }

        public Group(string groupName, User groupAdmin, List<User> groupMembers)
        {
            this.GroupName = groupName;
            this.Admin = groupAdmin;
            this.Members = groupMembers;
            int userCount = groupMembers.Count;
            int size = (userCount * (userCount - 1)) / 2; // size of the debtArray
            this._debtArray = new int[size];
            Expenses = new Dictionary<int, List<Expense>>();
            expenseHandler = new ExpenseHandler(Members);
        }

         private void EnlargeDebtArraySize(int newUserCount,int oldUserCount)
        {
            // Copy existing data to the new array
            _debtArray = expenseHandler.EnlargeDebtArraySize(newUserCount,oldUserCount,_debtArray);
        }

        public void AddMember(User user)
        {
            Members.Add(user);
            // Update the debt array size
            EnlargeDebtArraySize(Members.Count,Members.Count-1);

        }

        public void AddMember(List<User> newMembers)
        {
            int oldUserCount = Members.Count;
            Members.AddRange(newMembers);
            int newUserCount = Members.Count;
            // Update the debt array size
            EnlargeDebtArraySize(newUserCount,oldUserCount);
        }
        
        private void ReduceDebtArraySize(int newUserCount, int oldUserCount, List<int> removedUsers){

            _debtArray = expenseHandler.ReduceDebtArraySize(newUserCount,oldUserCount,removedUsers,_debtArray); 
        }

        public void RemoveMember(User user)
        {
            Members.Remove(user);
            List <int> removedUsers = new List<int>();
            removedUsers.Add(user.Id);
            // Update the debt array size
            ReduceDebtArraySize(Members.Count,Members.Count+1,removedUsers);

        }

        public void RemoveMembers(List<User> removedMembers){
            List <int> removedUsers = new List<int>();
            foreach(User user in removedMembers){
                Members.Remove(user);
                removedUsers.Add(user.Id);
            }
            // Update the debt array size
            ReduceDebtArraySize(Members.Count,Members.Count+removedMembers.Count,removedUsers);
        }
          
        //For Now i change it to ExpenseDTO 
        // public void AddExpense(Expense expense)
        // {
        //     User user = expense.payer;
        //     if (Expenses.ContainsKey(user.Id))
        //     {
        //         Expenses[user.Id].Add(expense);
        //     }
        //     else
        //     {
        //         Expenses[user.Id] = new List<Expense>();
        //         Expenses[user.Id].Add(expense);
        //     }
        //     ExpenseDto expenseDto = new ExpenseDto(expense);
            
        //     expenseHandler.AddExpense(expenseDto,_debtArray);

        // }

        // public void DeleteExpense(Expense expense)
        // {
        //     User user = expense.Payer;
        //     if (Expenses.ContainsKey(user.Id))
        //     {
        //         Expenses[user.Id].Remove(expense);
        //     }
        //     ExpenseDto expenseDto = new ExpenseDto(expense);
        //     expenseHandler.DeleteExpense(expenseDto,_debtArray);
        // }

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
