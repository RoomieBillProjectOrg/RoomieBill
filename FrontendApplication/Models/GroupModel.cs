namespace FrontendApplication.Models
{
    public class GroupModel
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public UserModel Admin { get; set; }
        public List<UserModel> Members { get; set; } = new List<UserModel>(); // Members of the group
        public ICollection<ExpenseModel> Expenses { get; set; } = new List<ExpenseModel>(); // Expenses of the group
        public int[] _debtArray; // 1D array to store debts
        public List<InviteModel> Invites { get; set; }

        // Empty constructor
        public GroupModel() { }
    }
}
