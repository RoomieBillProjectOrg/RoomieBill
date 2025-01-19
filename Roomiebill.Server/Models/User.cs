using System.ComponentModel.DataAnnotations;

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
        public List<Invite> Invites { get; set; } = [];
        public List<Group> GroupsUserIsMemberAt { get; set; } = [];

        public User() { }

        public User(string username, string email, string passwordHash, bool isSystemAdmin = false, bool isLoggedIn = false)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            IsSystemAdmin = isSystemAdmin;
            IsLoggedIn = isLoggedIn;
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

        public List<Group> GetGroups()
        {
            return GroupsUserIsMemberAt;
        }
    }
}
