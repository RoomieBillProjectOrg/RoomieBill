using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class SnoozeToPayDto
    {
        public string snoozeToUsername { get; set; }
        public string snoozeInfo { get; set; }
    }
}