using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendApplication.Models
{
    public class AnswerInviteByUserDto
    {
        public int InviteId { get; set; }
        public string InvitedUsername { get; set; }
        public bool IsAccepted { get; set; }

        public AnswerInviteByUserDto(int inviteId, string invitedUsername, bool isAccepted)
        {
            InviteId = inviteId;
            InvitedUsername = invitedUsername;
            IsAccepted = isAccepted;
        }

        public AnswerInviteByUserDto()
        {
        }
    }
}