namespace PaymentsDemo.Models;

public class UserBalance
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public int Version { get; set; }
}