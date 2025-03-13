using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Roomiebill.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }  // Store the hashed password
        public bool IsLoggedIn { get; set; } = false;
        public bool IsSystemAdmin { get; set; } = false;
        public string FirebaseToken { get; set; }
        public DateTime PasswordChangedDate { get; set; }

        [JsonIgnore]
        public List<Invite> Invites { get; set; } = [];
        
        [JsonIgnore]
        public List<Group> GroupsUserIsMemberAt { get; set; } = [];

        public User() { }

        public User(string username, string email, string passwordHash, bool isSystemAdmin = false, bool isLoggedIn = false, string firebaseToken = "")
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            IsSystemAdmin = isSystemAdmin;
            IsLoggedIn = isLoggedIn;
            FirebaseToken = firebaseToken;
            PasswordChangedDate = DateTime.Now;
            Invites = new List<Invite>();
            GroupsUserIsMemberAt = new List<Group>();
        }

        public void AddInvite(Invite invite)
        {
            Invites.Add(invite);
        }

        public void AddGroup(Group group)
        {
            GroupsUserIsMemberAt.Add(group);
        }

        public void RemoveGroup(Group group)
        {
            GroupsUserIsMemberAt.Remove(group);
        }
    }
}
