namespace DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;

public class ProductSummaryView
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = null!;
    public decimal TotalInventoryValue { get; set; }
}
