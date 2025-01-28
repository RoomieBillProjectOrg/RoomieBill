using Roomiebill.Server.Models;

public class DebtDto
{
    public User creditor { get; set; }
    public User debtor { get; set; }
    public decimal amount { get; set; }
}