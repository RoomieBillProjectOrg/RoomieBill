using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;

namespace Roomiebill.Server.DataAccessLayer
{
    public class DatabaseSeeder
    {
        private UserService _userService;
        private GroupService _groupService;
        private InviteService _inviteService;
        private GroupInviteMediatorService _groupInviteMediatorService;
        private ApplicationDbContext _context;
        public DatabaseSeeder(UserService userService, GroupService groupService, InviteService inviteService, GroupInviteMediatorService groupInviteMediatorService, ApplicationDbContext context)
        {
            _userService = userService;
            _groupService = groupService;
            _inviteService = inviteService;
            _groupInviteMediatorService = groupInviteMediatorService;
            _context = context;
        }
        public async Task SeedAsync()
        {
            // Check if the database is already seeded
            if (!_context.Users.Any())
            {
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Inbar", Email = "Inbar@bgu.ac.il", Password = "InbarPassword1!", FirebaseToken = "Test" });
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Metar", Email = "Metar@bgu.ac.il", Password = "MetarPassword2@", FirebaseToken = "Test" });
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Vladi", Email = "Vladi@bgu.ac.il", Password = "VladiPassword3#", FirebaseToken = "Test" });
                await _userService.RegisterUserAsync(new RegisterUserDto { Username = "Tal", Email = "Tal@bgu.ac.il", Password = "TalPassword4$", FirebaseToken = "Test" });

                // Log in all data users
                await _userService.LoginAsync(new LoginDto { Username = "Inbar", Password = "InbarPassword1!", FirebaseToken = "Test" });
                await _userService.LoginAsync(new LoginDto { Username = "Metar", Password = "MetarPassword2@", FirebaseToken = "Test" });
                await _userService.LoginAsync(new LoginDto { Username = "Vladi", Password = "VladiPassword3#", FirebaseToken = "Test" });
                await _userService.LoginAsync(new LoginDto { Username = "Tal", Password = "TalPassword4$", FirebaseToken = "Test" });

                // Create a new group for all data users - "Roomiebill"
                CreateNewGroupDto newGroupDetails = new CreateNewGroupDto
                {
                    AdminGroupUsername = "Inbar",
                    GroupMembersEmailsList = new List<string> { "Metar@bgu.ac.il", "Vladi@bgu.ac.il", "Tal@bgu.ac.il" },
                    GroupName = "Roomiebill"
                };

                Group group_Roomiebill = await _groupInviteMediatorService.CreateNewGroupSendInvitesAsync(newGroupDetails);

                // Invite users to the group
                //InviteToGroupByUsernameDto inviteDetails_Metar = new InviteToGroupByUsernameDto
                //{
                //    InviterUsername = "Inbar",
                //    InvitedUsername = "Metar",
                //    GroupId = group_Roomiebill.Id
                //};
                //InviteToGroupByUsernameDto inviteDetails_Vladi = new InviteToGroupByUsernameDto
                //{
                //    InviterUsername = "Inbar",
                //    InvitedUsername = "Vladi",
                //    GroupId = group_Roomiebill.Id
                //};
                //InviteToGroupByUsernameDto inviteDetails_Tal = new InviteToGroupByUsernameDto
                //{
                //    InviterUsername = "Inbar",
                //    InvitedUsername = "Tal",
                //    GroupId = group_Roomiebill.Id
                //};

                //await _groupService.InviteToGroupByUsername(inviteDetails_Metar);
                //await _groupService.InviteToGroupByUsername(inviteDetails_Vladi);
                //await _groupService.InviteToGroupByUsername(inviteDetails_Tal);

                // User Metar accepts the invitation
                var inviteMetar = _context.Invites.FirstOrDefault(i => i.Invited.Username.Equals("Metar"));
                AnswerInviteByUserDto answerDetails1_Metar = new AnswerInviteByUserDto
                {
                    InviteId = inviteMetar.Id,
                    InvitedUsername = "Metar",
                    IsAccepted = true
                };
                await _inviteService.AnswerInviteByUser(answerDetails1_Metar);

                // User Vladi accepts the invitation
                var inviteVladi = _context.Invites.FirstOrDefault(i => i.Invited.Username.Equals("Vladi"));
                AnswerInviteByUserDto answerDetails1_Vladi = new AnswerInviteByUserDto
                {
                    InviteId = inviteVladi.Id,
                    InvitedUsername = "Vladi",
                    IsAccepted = true
                };
                await _inviteService.AnswerInviteByUser(answerDetails1_Vladi);

                // User Tal accepts the invitation
                var inviteTal = _context.Invites.FirstOrDefault(i => i.Invited.Username.Equals("Tal"));
                AnswerInviteByUserDto answerDetails1_Tal = new AnswerInviteByUserDto
                {
                    InviteId = inviteTal.Id,
                    InvitedUsername = "Tal",
                    IsAccepted = true
                };
                await _inviteService.AnswerInviteByUser(answerDetails1_Tal);

                // Create new group for all data users - "Roomiebill_TestInvites"
                CreateNewGroupDto newGroupDetails_TestInvites = new CreateNewGroupDto
                {
                    AdminGroupUsername = "Inbar",
                    GroupMembersEmailsList = new List<string> { "Metar@bgu.ac.il", "Vladi@bgu.ac.il", "Tal@bgu.ac.il" },
                    GroupName = "Test Invites"
                };

                Group group_Roomiebill_TestInvites = await _groupInviteMediatorService.CreateNewGroupSendInvitesAsync(newGroupDetails_TestInvites);

                // Invite users to the group
                //InviteToGroupByUsernameDto inviteDetails_Metar_testInvite = new InviteToGroupByUsernameDto
                //{
                //    InviterUsername = "Inbar",
                //    InvitedUsername = "Metar",
                //    GroupId = group_Roomiebill_TestInvites.Id
                //};
                //InviteToGroupByUsernameDto inviteDetails_Vladi_testInvite = new InviteToGroupByUsernameDto
                //{
                //    InviterUsername = "Inbar",
                //    InvitedUsername = "Vladi",
                //    GroupId = group_Roomiebill_TestInvites.Id
                //};
                //InviteToGroupByUsernameDto inviteDetails_Tal_testInvite = new InviteToGroupByUsernameDto
                //{
                //    InviterUsername = "Inbar",
                //    InvitedUsername = "Tal",
                //    GroupId = group_Roomiebill_TestInvites.Id
                //};

                //await _groupService.InviteToGroupByUsername(inviteDetails_Metar_testInvite);
                //await _groupService.InviteToGroupByUsername(inviteDetails_Vladi_testInvite);
                //await _groupService.InviteToGroupByUsername(inviteDetails_Tal_testInvite);

                // Create an expense
                var expense = new Expense
                {
                    Amount = 500.0,
                    Description = "Power 10-12.2024",
                    IsPaid = false,
                    PayerId = group_Roomiebill.Members.First().Id,
                    GroupId = group_Roomiebill.Id,
                    ExpenseSplits = new List<ExpenseSplit>
                    {
                        new ExpenseSplit { UserId = group_Roomiebill.Members.ElementAt(0).Id, Amount = 125.0 },
                        new ExpenseSplit { UserId = group_Roomiebill.Members.ElementAt(1).Id, Amount = 125.0 },
                        new ExpenseSplit { UserId = group_Roomiebill.Members.ElementAt(2).Id, Amount = 125.0 },
                        new ExpenseSplit { UserId = group_Roomiebill.Members.ElementAt(3).Id, Amount = 125.0 }
                    }
                };

                // Add expense to group
                group_Roomiebill.AddExpense(expense);
                _context.SaveChanges();

                // //update an expense
                // var expenseToUpdate = _context.Expenses.FirstOrDefault(e => e.Description == "Power 10-12.2024");

                // if (expenseToUpdate != null)
                // {
                //     expenseToUpdate.Amount = 200.0;
                //     _context.Expenses.Update(expenseToUpdate);
                //     _context.SaveChanges();
                // }

                // Log out all data users
                await _userService.LogoutAsync("Inbar");
                await _userService.LogoutAsync("Metar");
                await _userService.LogoutAsync("Vladi");
                await _userService.LogoutAsync("Tal");
            }

            // await _userService.LoginAsync(new LoginDto { Username = "Vladi", Password = "VladiPassword3#", FirebaseToken = "" });

            // // Create a new group for all data users - "Roomiebill"
            // CreateNewGroupDto newGroupDetails2 = new CreateNewGroupDto
            // {
            //     AdminGroupUsername = "Inbar",
            //     GroupMembersUsernamesList = new List<string> { "TalTul" },
            //     GroupName = "NotificationTest537"
            // };

            // Group group_Roomiebill2 = await _groupInviteMediatorService.CreateNewGroupSendInvitesAsync(newGroupDetails2);

            // User Tal accepts the invitation
            // var inviteTal2 = _context.Invites.FirstOrDefault(i => i.Invited.Username.Equals("Tal") && i.Group.GroupName == "NotificationTest22");
            // AnswerInviteByUserDto answerDetails2_Tal = new AnswerInviteByUserDto
            // {
            //     InviteId = inviteTal2.Id,
            //     InvitedUsername = "Tal",
            //     IsAccepted = true
            // };
            // await _inviteService.AnswerInviteByUser(answerDetails2_Tal);


        }
    }
}
