namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class UpdatePasswordDto
    {
        public string? Username { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
