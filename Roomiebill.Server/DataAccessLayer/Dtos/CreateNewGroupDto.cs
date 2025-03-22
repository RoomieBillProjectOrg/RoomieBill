namespace Roomiebill.Server.DataAccessLayer.Dtos
{
    public class CreateNewGroupDto
    {
        public required string GroupName { get; set; }
        public required string AdminGroupUsername { get; set; }

        public List<string>? GroupMembersEmailsList { get; set; }
    }
}