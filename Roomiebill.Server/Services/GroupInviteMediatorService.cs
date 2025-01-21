using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services
{
    public class GroupInviteMediatorService
    {
        
        private readonly GroupService _groupService;
        private readonly InviteService _inviteService;

        public GroupInviteMediatorService(GroupService groupService, InviteService inviteService)
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