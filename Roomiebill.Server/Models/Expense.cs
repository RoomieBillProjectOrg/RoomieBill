
using System.ComponentModel.DataAnnotations.Schema;

namespace Roomiebill.Server.Models
{
    public class Expense
    {
        // THIS CLASS IS IN COMMENT BECAUSE OF ERRORS IN DB BECAUSE OF THE NOT MAPPED PROPERTY IN ApplicationDbContext CLASS


        public int Id { get; set; }
        //public double Amount { get; set; }
        //public string Description { get; set; }
        //public bool IsPaid { get; set; }
        //public User Payer { get; set; }

        //[NotMapped]
        //public Dictionary<int, double> SplitBetween { get; set; } //<UserId, percentage> maps between user id and percentage of the pay

    }
}
