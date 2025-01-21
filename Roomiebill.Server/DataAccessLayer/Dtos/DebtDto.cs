public class DebtDto
{
    public int OwedByUserId { get; set; }
    public string OwedByUserName { get; set; } // Optional: For user display
    public double Amount { get; set; }
}