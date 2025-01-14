
using Roomiebill.Server.Enums;

namespace Roomiebill.Server.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public string InviterUsername { get; set; }
        public string InviteeUsername { get; set; }
        public int GroupId { get; set; }
        public Status Status { get; set; }
        public DateTime Date { get; set; }

        public Invite(string inviterUsername, string inviteeUsername, int groupId)
        {
            InviterUsername = inviterUsername;
            InviteeUsername = inviteeUsername;
            GroupId = groupId;
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