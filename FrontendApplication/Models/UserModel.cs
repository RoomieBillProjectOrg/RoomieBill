namespace FrontendApplication.Models
{
    public class UserModel
    {
        public int Id { get; set; } = 1;
        public string Username { get; set; } = "DefaultUser";
        public string Email { get; set; } = "DefaultUser@email.com";
        public string PasswordHash { get; set; }  = ""; // Store the hashed password
        public string BitLink { get; set; }
        public bool IsLoggedIn { get; set; } = false;
        public bool IsSystemAdmin { get; set; } = false;
        public string FirebaseToken { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public List<InviteModel> Invites { get; set; } = [];
        public List<GroupModel> GroupsUserIsMemberAt { get; set; } = [];

        public UserModel() { }
        
    }
}
