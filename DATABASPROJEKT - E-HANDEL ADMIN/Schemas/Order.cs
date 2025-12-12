using System.ComponentModel.DataAnnotations;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;

public class Order
{
    // PK
    public int OrderId { get; set; }

    // FK
    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    // Navigation
    public Customer? Customer { get; set; }
    
    public List<OrderRow> OrderRows { get; set; } = [];
}
