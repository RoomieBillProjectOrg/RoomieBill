using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Services.Interfaces;

namespace Roomiebill.Server.Services
{
    public class BillingService : IBillingService
    {
        // Handle bills, their splitting, and payments.

        private readonly ApplicationDbContext _context;

        public BillingService(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}
