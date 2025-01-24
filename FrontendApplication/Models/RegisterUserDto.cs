using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendApplication.Models
{
    public class RegisterUserDto
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string firebaseToken { get; set; }
    }
}
