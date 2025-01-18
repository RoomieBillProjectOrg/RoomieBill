namespace FrontendApplication.Models
{
    public class UpdatePasswordDto
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string VerifyNewPassword { get; set; }

        // Empty constructor
        public UpdatePasswordDto() { }
    }
}
