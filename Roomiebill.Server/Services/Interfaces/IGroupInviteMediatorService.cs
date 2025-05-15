using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services.Interfaces
{
    public interface IGroupInviteMediatorService
    {
        Task<Group> CreateNewGroupSendInvitesAsync(CreateNewGroupDto group);
    }
}
