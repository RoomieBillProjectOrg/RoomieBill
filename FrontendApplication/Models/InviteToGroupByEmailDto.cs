namespace FrontendApplication.Models
{
    public class InviteToGroupByEmailDto
    {
        public string InviterUsername { get; set; }
        public string Email { get; set; }
        public int GroupId { get; set; }
    }
}
