﻿namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirebaseToken { get; set; }
    }
}
