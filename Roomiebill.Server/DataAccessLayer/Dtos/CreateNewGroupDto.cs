namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class CreateNewGroupDto
    {
        public string GroupName { get; set; }
        public string AdminGroupUsername { get; set; }

        public List<string> GroupMembersPhoneNumbersList { get; set; }
    }
}