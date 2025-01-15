using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;

namespace Roomiebill.Server.DataAccessLayer
{
    public class DatabaseSeeder
    {
        private UserService _userService;
        private GroupService _groupService;
        private ApplicationDbContext _context;
        public DatabaseSeeder(UserService userService, GroupService groupService, ApplicationDbContext context)
        {
            _userService = userService;
            _groupService = groupService;
            _context = context;
        }
        public async Task SeedAsync()
        {
            // Check if the database is already seeded
            if (!_context.Users.Any())
            {
                // Add initial data if not already present
                var user1 = new User("Inbar", "inbar@bgu.ac.il", "InbarPassword1!", true);
                var user2 = new User("Metar", "Metar@bgu.ac.il", "MetarPassword2@", true);
                var user3 = new User("Vladi", "Vladi@bgu.ac.il", "VladiPassword3#", true);
                var user4 = new User("Tal", "Tal@bgu.ac.il", "TalPassword4$", true);

                _context.Users.AddRange(user1, user2, user3, user4);

                _context.SaveChanges();
            }

            if (!_context.Groups.Any())
            {
                CreateNewGroupDto newGroupDetails = new CreateNewGroupDto
                {
                    AdminGroupUsername = "Inbar",
                    GroupMembersPhoneNumbersList = new List<string> { "Metar", "Vladi" },
                    GroupName = "Roomiebill"
                };

                await _groupService.CreateNewGroupAsync(newGroupDetails);

                _context.SaveChanges();
            }

            if (!_context.Invites.Any())
            {
                InviteToGroupByUsernameDto inviteDetails = new InviteToGroupByUsernameDto
                {
                    InviterUsername = "Inbar",
                    InvitedUsername = "Metar",
                    GroupId = 1
                };

                await _groupService.InviteToGroupByUsername(inviteDetails);

                _context.SaveChanges();

            }
        }
    }
}
