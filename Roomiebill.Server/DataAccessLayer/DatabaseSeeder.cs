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

                //_context.SaveChanges();
                //}

                // Log in all data users
                await _userService.LoginAsync(new LoginDto { Username = "Inbar", Password = "InbarPassword1!" });
                await _userService.LoginAsync(new LoginDto { Username = "Metar", Password = "MetarPassword2@" });
                await _userService.LoginAsync(new LoginDto { Username = "Vladi", Password = "VladiPassword3#" });
                await _userService.LoginAsync(new LoginDto { Username = "Tal", Password = "TalPassword4$" });


                //if (!_context.Groups.Any())
                //{
                CreateNewGroupDto newGroupDetails = new CreateNewGroupDto
                {
                    AdminGroupUsername = "Inbar",
                    GroupMembersUsernamesList = new List<string> { "Metar" },
                    GroupName = "Roomiebill"
                };

                await _groupService.CreateNewGroupAsync(newGroupDetails);

                //_context.SaveChanges();
                //}

                //if (!_context.Invites.Any())
                //{

                InviteToGroupByUsernameDto inviteDetails = new InviteToGroupByUsernameDto
                {
                    InviterUsername = "Inbar",
                    InvitedUsername = "Tal",
                    GroupId = 3
                };

                await _groupService.InviteToGroupByUsername(inviteDetails);

                //}

                // Ensure the group exists
                var group = _context.Groups.FirstOrDefault(g => g.GroupName == "Test Group");
                //if (group == null)
                //{
                var user1 = _context.Users.FirstOrDefault(u => u.Username == "Inbar");
                var user2 = _context.Users.FirstOrDefault(u => u.Username == "Metar");
                var user3 = _context.Users.FirstOrDefault(u => u.Username == "Vladi");
                var user4 = _context.Users.FirstOrDefault(u => u.Username == "Tal");

                await _groupService.CreateNewGroupAsync(new CreateNewGroupDto
                {
                    GroupName = "Test Group",
                    AdminGroupUsername = "Inbar",
                    GroupMembersUsernamesList = new List<string> { "Metar", "Vladi", "Tal" }
                });

                //}

                // Create an expense
                var expense = new Expense
                {
                    Amount = 500.0,
                    Description = "Tal's Mesibat Ravakim",
                    IsPaid = false,
                    PayerId = group.Members.First().Id,
                    GroupId = group.Id,
                    ExpenseSplits = new List<ExpenseSplit>
                    {
                        new ExpenseSplit { UserId = group.Members.ElementAt(0).Id, Percentage = 25.0 },
                        new ExpenseSplit { UserId = group.Members.ElementAt(1).Id, Percentage = 25.0 },
                        new ExpenseSplit { UserId = group.Members.ElementAt(2).Id, Percentage = 25.0 },
                        new ExpenseSplit { UserId = group.Members.ElementAt(3).Id, Percentage = 25.0 }
                    }
                };

                _context.Expenses.Add(expense);
                _context.SaveChanges();

                //update an expense
                var expenseToUpdate = _context.Expenses.FirstOrDefault(e => e.Description == "Tal's Mesibat Ravakim");

                if (expenseToUpdate != null)
                {
                    expenseToUpdate.Amount = 200.0;
                    _context.Expenses.Update(expenseToUpdate);
                    _context.SaveChanges();
                }

                // Log out all data users
                await _userService.LogoutAsync("Inbar");
                await _userService.LogoutAsync("Metar");
                await _userService.LogoutAsync("Vladi");
                await _userService.LogoutAsync("Tal");
            }
        }
    }
}