
namespace Roomiebill.Server.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public User Inviter { get; set; }
        public User Invitee { get; set; }
        public Group Group { get; set; }
        // public Status Status { get; set; }
        public DateTime Date { get; set; }
    }
}