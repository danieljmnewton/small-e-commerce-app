using System.ComponentModel.DataAnnotations;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;

public class OrderRow
{
    // PK
    public int OrderRowId { get; set; }

    // FK
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    // Navigation
    public Order? Order { get; set; }

    public Product? Product { get; set; }
}
