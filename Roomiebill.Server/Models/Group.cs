using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        [Column("DebtArray")] // Save as JSON string in the database
        public string DebtArray
        {
            get => JsonSerializer.Serialize(_debtArray);
            set => _debtArray = string.IsNullOrEmpty(value) 
                                ? [] 
                                : JsonSerializer.Deserialize<double[]>(value) ?? [];
        }

        [JsonIgnore]
        public List<Invite> Invites { get; set; }

        [NotMapped]
        [JsonIgnore]
        public ExpenseHandler expenseHandler { get; set; }

        [NotMapped]
        private double[] _debtArray = new double[0]; // Backing field for the serialized DebtArray

        public Group()
        {
            // Default constructor required by EF Core
            GroupName = "Default Name";
            InitializeCollections();
            InitializeDebtArray();
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

            if (!Members.Contains(groupAdmin))
            {
                Members.Add(groupAdmin); // Add the admin to the members list
            }
            
            InitializeCollections();
            InitializeDebtArray();
        }

        private void InitializeCollections()
        {
            Invites = new List<Invite>();
            Expenses = new List<Expense>();
            expenseHandler = new ExpenseHandler(Members);
        }

        private void InitializeDebtArray()
        {
            int memberCount = Members.Count;
            int size = (memberCount * (memberCount - 1)) / 2; // Calculate size for 1D representation
            _debtArray = new double[size];
        }

        public void InitializeNonPersistentProperties()
        {
            expenseHandler = new ExpenseHandler(Members);
        }

        public void AddExpense(Expense expense)
        {
            expenseHandler.AddExpense(expense, _debtArray);
            Expenses.Add(expense);
        }

        public void EnlargeDebtArraySize(int newUserCount, int oldUserCount)
        {
            // Copy existing data to the new array
            _debtArray = expenseHandler.EnlargeDebtArraySize(newUserCount, oldUserCount, _debtArray);
        }

        public void AddMember(User user)
        {
            Members.Add(user);

            // Update the debt array size
            EnlargeDebtArraySize(Members.Count, Members.Count - 1);

            // Add the user to the user index map
            expenseHandler.AddUserToUserIndexMap(user.Id);
        }

        public void AddMember(List<User> newMembers)
        {
            int oldUserCount = Members.Count;
            Members.AddRange(newMembers);
            int newUserCount = Members.Count;
            // Update the debt array size
            EnlargeDebtArraySize(newUserCount, oldUserCount);
        }

        private void ReduceDebtArraySize(int newUserCount, int oldUserCount, List<int> removedUsers)
        {
            _debtArray = expenseHandler.ReduceDebtArraySize(newUserCount, oldUserCount, removedUsers, _debtArray);
        }

        public void RemoveMember(User user)
        {
            Members.Remove(user);
            List<int> removedUsers = new List<int>();
            removedUsers.Add(user.Id);
            // Update the debt array size
            ReduceDebtArraySize(Members.Count, Members.Count + 1, removedUsers);
        }

        public void AddInvite(Invite invite)
        {
            Invites.Add(invite);
        }

        public void RemoveMembers(List<User> removedMembers)
        {
            List<int> removedUsers = new List<int>();
            foreach (User user in removedMembers)
            {
                Members.Remove(user);
                removedUsers.Add(user.Id);
            }
            // Update the debt array size
            ReduceDebtArraySize(Members.Count, Members.Count + removedMembers.Count, removedUsers);
        }

        public void updateExpense(Expense oldExpense, Expense newExpense)
        {
            Expense updatedExpense = expenseHandler.UpdateExpense(oldExpense, newExpense, _debtArray);
            bool flag = Expenses.Remove(oldExpense);
            Expenses.Add(updatedExpense);
        }

        public void DeleteExpense(Expense expense)
        {
            expenseHandler.DeleteExpense(expense, _debtArray);
            Expenses.Remove(expense);
        }

        public double[] getDebtArray()
        {
            return _debtArray;
        }

        public double getDebtBetweenUsers(int user1Id, int user2Id)
        {
            return expenseHandler.GetDebtBetweenIndex(user1Id, user2Id, _debtArray);
        }

        public List<DebtDto> GetDebtsForUser(int userId)
        {
            List<DebtDto> debts = new List<DebtDto>();
            foreach (User member in Members)
            {
                if (member.Id != userId)
                {
                    double amount = expenseHandler.GetDebtBetweenIndex(member.Id, userId, _debtArray);
                    if (amount > 0.01) // Using small epsilon for double comparison
                    {
                        DebtDto debt = new DebtDto();
                        debt.amount = (decimal)amount; // Convert to decimal for currency values
                        debt.creditor = getUserById(userId);
                        debt.debtor = getUserById(member.Id);
                        debts.Add(debt);
                    }
                }
            }
            return debts;
        }

        public List<DebtDto> GetDebtsOwedByUser(int userId)
        {
            List<DebtDto> debts = new List<DebtDto>();
            foreach (User member in Members)
            {
                if (member.Id != userId)
                {
                    double amount = expenseHandler.GetDebtBetweenIndex(userId, member.Id, _debtArray);
                    if (amount > 0.01) // Using small epsilon for double comparison
                    {
                        DebtDto debt = new DebtDto();
                        debt.amount = (decimal)amount; // Convert to decimal for currency values
                        debt.creditor = getUserById(member.Id);
                        debt.debtor = getUserById(userId);
                        debts.Add(debt);
                    }
                }
            }
            return debts;
        }
       
        public void SettleDebt(decimal amount, int creditor, int debtor)
        {
            expenseHandler.SettleDebt(creditor, debtor, _debtArray);
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

        public User getUserById(int userId)
        {
            if(userId < 0)
            {
                throw new Exception("User with wrong id");
            }
            return Members.FirstOrDefault(m => m.Id == userId);
        }

        public bool CanUserExitGroup(int userId)
        {
            // Check if user has any debts or is owed any money
            var debtsOwed = GetDebtsOwedByUser(userId);
            var debtsToReceive = GetDebtsForUser(userId);

            return debtsOwed.Count == 0 && debtsToReceive.Count == 0;
        }

        public bool CanDeleteGroup()
        {
            // Check for any debts between any members
            foreach (var member in Members)
            {
                // If any member has debts or is owed money, group cannot be deleted
                if (!CanUserExitGroup(member.Id))
                    return false;
            }
            return true;
        }
    }
}
