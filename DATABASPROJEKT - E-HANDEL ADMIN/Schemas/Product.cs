using System.ComponentModel.DataAnnotations;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;

public class Product
{
    // PK
    public int ProductId { get; set; }

    // FK
    public int CategoryId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(250)]
    public string Description { get; set; } = null!;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    // Navigation
    public Category? Category { get; set; }
}
