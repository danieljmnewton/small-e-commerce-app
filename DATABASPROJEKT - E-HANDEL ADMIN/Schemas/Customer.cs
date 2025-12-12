using System.ComponentModel.DataAnnotations;
using DATABASPROJEKT___E_HANDEL_ADMIN.Services;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;

public class Customer
{
    // PK
    public int CustomerId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    private string _email = null!;

    [Required, MaxLength(250)]
    public string Email
    {
        get => EncryptionService.Decrypt(_email);
        set => _email = EncryptionService.Encrypt(value);
    }
    
    [MaxLength(500)]
    public string? EmailHash { get; set; }

    [MaxLength(250)]
    public string? City { get; set; }
    
    [MaxLength(500)]
    public string PasswordHash { get; set; } = null!;

    [MaxLength(100)]
    public string PasswordSalt { get; set; } = null!;

    // Navigation
    public List<Order> Orders { get; set; } = [];
}
