
using Roomiebill.Server.Enums;

namespace Roomiebill.Server.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public User Inviter { get; set; }
        public User Invited { get; set; }
        public Group Group { get; set; }
        public Status Status { get; set; }
        public DateTime Date { get; set; }

        // Parameterless constructor for EF
        public Invite() { }

        public Invite(User inviter, User invited, Group group)
        {
            Inviter = inviter;
            Invited = invited;
            Group = group;
            Status = Status.Pending;
            Date = DateTime.Now;
        }

        public void AcceptInvite()
        {
            Status = Status.Accepted;
        }

        public void RejectInvite()
        {
            Status = Status.Rejected;
        }
    }
}