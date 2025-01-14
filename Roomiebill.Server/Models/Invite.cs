
using Roomiebill.Server.Enums;

namespace Roomiebill.Server.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public int InviterId { get; set; }
        public int InviteeId { get; set; }
        public int GroupId { get; set; }
        public Status Status { get; set; }
        public DateTime Date { get; set; }

        public Invite(int inviterId, int inviteeId, int groupId)
        {
            InviterId = inviterId;
            InviteeId = inviteeId;
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