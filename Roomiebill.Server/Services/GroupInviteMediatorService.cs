using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;

namespace Roomiebill.Server.Services
{
    public class GroupInviteMediatorService : IGroupInviteMediatorService
    {
        
        private readonly IGroupService _groupService;
        private readonly IInviteService _inviteService;

        public GroupInviteMediatorService(IGroupService groupService, IInviteService inviteService)
        {
            _groupService = groupService;
            _inviteService = inviteService;
        }

        public async Task<Group> CreateNewGroupSendInvitesAsync(CreateNewGroupDto group)
        {
            var newGroup = await _groupService.CreateNewGroupAsync(group);
            await _inviteService.InviteToGroupByUsernamesList(group, newGroup.Id);

            return newGroup;
        }
    }
}
