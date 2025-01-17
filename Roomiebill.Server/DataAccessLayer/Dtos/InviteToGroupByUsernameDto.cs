using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Roomiebill.Server.Common.Enums;

namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class InviteToGroupByUsernameDto
    {
        public int Id { get; set; }
        public string InviterUsername { get; set; }
        public string InvitedUsername { get; set; }
        public int GroupId { get; set; }
        public Status Status { get; set; }
        public DateTime Date { get; set; }
    }
}