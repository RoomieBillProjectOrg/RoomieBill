using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.Common.Notificaiton;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Common.Notification;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public class InviteFacade : IInviteFacade
    {
        private readonly IApplicationDbContext _applicationDbs;
        private ILogger<InviteFacade> _logger;
        private readonly IUserFacade _userFacade;
        private readonly IGroupFacade _groupFacade;

        public InviteFacade(IApplicationDbContext inviteDb, ILogger<InviteFacade> logger, IUserFacade userFacade, IGroupFacade groupFacade)
        {
            _applicationDbs = inviteDb;
            _logger = logger;
            _userFacade = userFacade;
            _groupFacade = groupFacade;
        }

        /// <summary>
        /// This method invites a user to a group by their username. If the inviter, invited or group do not exist in the system, an exception is thrown.
        /// </summary>
        /// <param name="inviter_username"></param>
        /// <param name="email"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task InviteToGroupByEmail(string inviter_username, string emailTo, int groupId)
        {
            _logger.LogInformation($"Inviting user with email {emailTo} to group with id {groupId}.");

            User? inviter = await _userFacade.GetUserByUsernameAsync(inviter_username);

            if (inviter == null)
            {
                _logger.LogError($"Error when trying to invite user to group: inviter with username {inviter_username} does not exist in the system.");
                throw new Exception($"Error when trying to invite user to group: inviter with username {inviter_username} does not exist in the system.");
            }

            User? invited = await _userFacade.GetUserByEmailAsync(emailTo);

            if (invited == null)
            {
                _logger.LogError($"Error when trying to invite user to group: invited with email {emailTo} does not exist in the system.");
                throw new Exception($"Error when trying to invite user to group: invited with email {emailTo} does not exist in the system.");
            }

            Group? group = await _applicationDbs.GetGroupByIdAsync(groupId);

            if (group == null)
            {
                _logger.LogError($"Error when trying to invite user to group: group with id {groupId} does not exist in the system.");
                throw new Exception($"Error when trying to invite user to group: group with id {groupId} does not exist in the system.");
            }

            if (IsInviteForUserExistInGroup(invited, group))
            {
                _logger.LogError($"Error when trying to invite user to group: user with email {emailTo} is already invited to group with id {groupId}.");
                throw new Exception($"Error when trying to invite user to group: user with email {emailTo} is already invited to group with id {groupId}.");
            }

            if (!_groupFacade.IsUserInGroup(inviter, group))
            {
                _logger.LogError($"Error when trying to invite user to group: user with email {emailTo} is not a member of group with id {groupId}.");
                throw new Exception($"Error when trying to invite user to group: user with email {emailTo} is not a member of group with id {groupId}.");
            }

            if (_groupFacade.IsUserInGroup(invited, group))
            {
                _logger.LogError($"Error when trying to invite user to group: user with email {emailTo} is already a member of group with id {groupId}.");
                throw new Exception($"Error when trying to invite user to group: user with email {emailTo} is already a member of group with id {groupId}.");
            }

            Invite invite = new Invite
            {
                Inviter = inviter,
                Email = emailTo,
                Group = group,
                Status = Status.Pending,
                Date = DateTime.Now
            };


            await AddInviteToinvited(invited, invite);

            await AddInviteToGroup(group, invite);

            // Send email notification
            await SendEmailNotificationAsync(inviter_username, emailTo, group);

            _logger.LogInformation($"User with email {emailTo} has been invited to group with id {groupId}.");
        }

        public async Task InviteToGroupByEmailsList(string inviter_username, List<string> invited_emails, int groupId)
        {
            foreach (string invited_email in invited_emails)
            {
                await InviteToGroupByEmail(inviter_username, invited_email, groupId);
            }
        }

        public async Task AnswerInviteByUser(int inviteId, bool isAccepted)
        {
            await AnswerInviteByUserInUser(inviteId, isAccepted);
            await AnswerInviteByUserInGroup(inviteId);
        }

        #region Helper methods

        /// <summary>
        /// This method checks if an invite for a user already exists in a group.
        /// </summary>
        /// <param name="invited"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private bool IsInviteForUserExistInGroup(User invited, Group group)
        {
            return group.Invites.Any(i => i.Invited == invited);
        }

        private async Task AddInviteToGroup(Group group, Invite invite)
        {
            _logger.LogInformation($"Adding invite to group with id {group.Id}.");

            group.AddInvite(invite);

            await _applicationDbs.UpdateGroupAsync(group);

            _logger.LogInformation($"Invite has been added to group with id {group.Id}.");
        }

        /// <summary>
        /// Add an invite to the invited user.
        /// </summary>
        /// <param name="invited"></param>
        /// <param name="inv"></param>
        /// <returns></returns>
        private async Task AddInviteToinvited(User invited, Invite inv)
        {
            _logger.LogInformation($"Adding invite to user {invited.Username}");

            invited.AddInvite(inv);

            await _applicationDbs.UpdateUserAsync(invited);

            _logger.LogInformation($"Invite added to user {invited.Username}");
        }

        /// <summary>
        /// Answer an invite by the user.
        /// </summary>
        /// <param name="inviteId"></param>
        /// <param name="isAccepted"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<Invite> AnswerInviteByUserInUser(int inviteId, bool isAccepted)
        {
            _logger.LogInformation($"Answering invite with id {inviteId}");
            var invite = await _applicationDbs.GetInviteByIdAsync(inviteId);

            // here is the place to delete the invite from the user if wanted.
            if (invite == null)
            {
                _logger.LogError($"Invite with id {inviteId} does not exist");
                throw new Exception("Invite does not exist");
            }
            if (invite.Status != Status.Pending)
            {
                _logger.LogError($"Invite with id {inviteId} is not pending");
                throw new Exception("Invite is not pending");
            }

            if (isAccepted)
            {
                invite.AcceptInvite();
            }
            else
            {
                invite.RejectInvite();
            }

            await _applicationDbs.UpdateInviteAsync(invite);

            _logger.LogInformation($"Invite with id {inviteId} answered");

            return invite;
        }

        /// <summary>
        /// This method answers an invite by a user. If the invite does not exist, an exception is thrown.
        /// </summary>
        /// <param name="inviteId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task AnswerInviteByUserInGroup(int inviteId)
        {
            _logger.LogInformation($"Answering invite with id {inviteId}.");

            Invite? invite = await _applicationDbs.GetInviteByIdAsync(inviteId);

            if (invite == null)
            {
                _logger.LogError($"Error when trying to answer invite: invite with id {inviteId} does not exist in the system.");
                throw new Exception($"Error when trying to answer invite: invite with id {inviteId} does not exist in the system.");
            }

            if (invite.Status == Status.Accepted)
            {
                await _groupFacade.AddMemberToGroupAsync(invite.Invited, invite.Group);
            }

            _logger.LogInformation($"Invite with id {inviteId} has been answered.");
        }

        private async Task SendEmailNotificationAsync(string inviter_username, string email, Group group)
        {
            string subject = "You have been invited to a new group";
            string body = $"You have been invited to a new group {group.GroupName} by {inviter_username}.";
            await EmailNotificationHandler.SendEmailAsync(email, subject, body);
        }

        #endregion
    }
}
