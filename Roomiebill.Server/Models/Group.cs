
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;

namespace Roomiebill.Server.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Group name cannot exceed 20 characters.")]

        public string GroupName { get; set; }

        public User Admin { get; set; }

        public List<User> Members { get; set; } = new List<User>(); // Members of the group

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>(); // Expenses of the group

        [NotMapped]
        private int[] _debtArray; // 1D array to store debts

        public List<Invite> Invites { get; set; }
        
        [NotMapped]
        [JsonIgnore]
        ExpenseHandler expenseHandler { get; set; }

        public Group()
        {
            // Default constructor required by EF Core

            Members = new List<User>();
            Invites = new List<Invite>();
            GroupName = "Default Name";
            _debtArray = new int[1];
            Expenses = new List<Expense>();
            expenseHandler = new ExpenseHandler(Members);
        }

        public Group(string groupName, User groupAdmin, List<User> members)
        {
             if (string.IsNullOrWhiteSpace(groupName))
                throw new ArgumentException("Group name cannot be empty or null.", nameof(groupName));

            if (groupAdmin == null)
                throw new ArgumentNullException(nameof(groupAdmin), "Admin cannot be null.");

            GroupName = groupName;
            Admin = groupAdmin;
            Members = new List<User>(members); // Defensive copy
            Members.Add(groupAdmin); // Add the admin to the members list
            Invites = new List<Invite>();
            _debtArray = new int[(members.Count * (members.Count - 1)) / 2]; // Calculate size for 1D representation
            Expenses = new List<Expense>();
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

        public void AddInvite(Invite invite)
        {
            Invites.Add(invite);
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
        //TODO: check for operation completion
        public void AddExpense(Expense expense){
            expenseHandler.AddExpense(expense,_debtArray);
            Expenses.Add(expense);
        }
        public void updateExpense(Expense oldExpense, Expense newExpense){
            Expense updatedExpense = expenseHandler.UpdateExpense(oldExpense, newExpense, _debtArray);
            bool flag = Expenses.Remove(oldExpense);
            Expenses.Add(updatedExpense);
        }   
        //TODO: check for operation completion
        public void DeleteExpense(Expense expense){
            expenseHandler.DeleteExpense(expense,_debtArray);
            Expenses.Remove(expense);
        }
        public int [] getDebtArray(){
            return _debtArray;
        }   
        public int getDebtBetweenUsers(int user1Id, int user2Id){
            return expenseHandler.GetDebtBetweenIndex(user1Id,user2Id,_debtArray);
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
