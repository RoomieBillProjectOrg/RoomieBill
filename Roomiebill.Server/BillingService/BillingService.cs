using Roomiebill.Server.DataAccessLayer;

namespace Roomiebill.Server.BillingService
{
    public class BillingService
    {
        // Handle bills, their splitting, and payments.

        private readonly ApplicationDbContext _context;

        public BillingService(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}
