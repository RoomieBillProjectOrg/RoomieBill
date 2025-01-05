using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public class GroupFacade
    {
        public List<Group> Groups { get; set; }

        public GroupFacade()
        {
            Groups = new List<Group>();
        }

        /**/
        public void AddGroup(Group group)
        {
            Groups.Add(group);
        }
        //create a new group
        public void CreateGroup(int id,string name, string description, User admin)
        {
            Group group = new Group(id, name, description, admin);
            Groups.Add(group);
        }

        public void InviteToGroupByUsername(int inviter_id, string invited_username, int groupId)
        {

        }
    }
}