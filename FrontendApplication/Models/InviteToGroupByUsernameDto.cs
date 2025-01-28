using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontendApplication.Models
{
    public class InviteToGroupByUsernameDto
    {
        public string InviterUsername { get; set; }
        public string InvitedUsername { get; set; }
        public int GroupId { get; set; }
    }
}