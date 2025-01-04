using Roomiebill.Server.DataAccessLayer;

namespace Roomiebill.Server.Services
{
    public class GroupService
    {
        // Logic for managing groups and members.


        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}
