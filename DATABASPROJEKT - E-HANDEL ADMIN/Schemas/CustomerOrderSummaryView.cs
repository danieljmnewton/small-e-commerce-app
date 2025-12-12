namespace DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;

public class CustomerOrderSummaryView
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? City { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}
