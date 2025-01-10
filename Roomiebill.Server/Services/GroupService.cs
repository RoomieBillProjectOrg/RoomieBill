using Microsoft.AspNetCore.Identity;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Facades;

namespace Roomiebill.Server.Services
{
    public class GroupService
    {
        private readonly GroupFacade _groupFacade;
        private readonly UserFacade _userFacade;

        public GroupService(GroupsDb groupsDb, ILogger<GroupFacade> groupFacadeLogger, UserService userService)
        {
            _userFacade = userService._userFacade;
            _groupFacade = new GroupFacade(groupsDb, groupFacadeLogger, _userFacade);
        }
    }
}
