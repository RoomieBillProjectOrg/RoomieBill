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
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Inbar", Email = "inbar@bgu.ac.il", Password = "InbarPassword1!" });
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Metar", Email = "Metar@bgu.ac.il", Password = "MetarPassword2@" });
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Vladi", Email = "Vladi@bgu.ac.il", Password = "VladiPassword3#" });
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Tal", Email = "Tal@bgu.ac.il", Password = "TalPassword4$" });

                _context.SaveChanges();
            }

            if (!_context.Groups.Any())
            {
                CreateNewGroupDto newGroupDetails = new CreateNewGroupDto
                {
                    AdminGroupUsername = "Inbar",
                    GroupMembersPhoneNumbersList = new List<string> { "Metar" },
                    GroupName = "Roomiebill"
                };

                await _groupService.CreateNewGroupAsync(newGroupDetails);

                _context.SaveChanges();
            }

            if (!_context.Invites.Any())
            {
                await _userService.LoginAsync(new LoginDto { Username = "Tal", Password = "TalPassword4$" });

                InviteToGroupByUsernameDto inviteDetails = new InviteToGroupByUsernameDto
                {
                    InviterUsername = "Inbar",
                    InvitedUsername = "Tal",
                    GroupId = 3
                };

                await _groupService.InviteToGroupByUsername(inviteDetails);

                _context.SaveChanges();

            }
        }
    }
}
