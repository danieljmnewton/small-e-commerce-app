using System.ComponentModel.DataAnnotations;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;

public class Category
{
    // PK
    public int CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }

    // Navigation
    public List<Product> Products { get; set; } = [];
}
