
namespace FrontendApplication.Models
{
    public class DebtModel
    {
        public UserModel creditor { get; set; }
        public UserModel debtor { get; set; }
        public decimal amount { get; set; }
    }
}