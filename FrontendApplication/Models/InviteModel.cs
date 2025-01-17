using Roomiebill.FrontendApplication.Models;

namespace FrontendApplication.Models
{
    public class InviteModel
    {
        public int Id { get; set; }
        public UserModel Inviter { get; set; }
        public UserModel Invited { get; set; }
        public GroupModel Group { get; set; }
        public StatusModel Status { get; set; }
        public DateTime Date { get; set; }

        // Parameterless constructor for EF
        public InviteModel() { }
    }
}
