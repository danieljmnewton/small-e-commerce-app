using DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;
using Microsoft.EntityFrameworkCore;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Services;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    // CREATE with hashed password and email
    public async Task<(bool success, string message, Customer? customer)> CreateAsync(
        string name,
        string email,
        string password,
        string? city)
    {
        
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name is required.", null);

        if (name.Length > 100)
            return (false, "Name cannot exceed 100 characters.", null);

        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required.", null);

        if (email.Length > 250)
            return (false, "Email cannot exceed 250 characters.", null);

        if (!email.Contains('@') || !email.Contains('.'))
            return (false, "Invalid email address.", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required.", null);

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters.", null);

        if (await _context.Customers.AnyAsync(c => c.Email == email))
            return (false, "A customer with this email already exists.", null);
        
        var (passwordHash, passwordSalt) = HashingService.HashPassword(password);
        var emailHash = HashingService.HashValue(email, passwordSalt);

        var customer = new Customer
        {
            Name = name.Trim(),
            Email = email.Trim().ToLower(),
            EmailHash = emailHash,
            City = city?.Trim(),
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return (true, "Customer created.", customer);
    }

    // READ - Get by ID
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    // READ - Get all with pagination and sorting
    public async Task<(List<Customer> items, int totalCount, int totalPages)> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string sortBy = "Name",
        bool ascending = true)
    {
        var query = _context.Customers.AsQueryable();
        
        query = sortBy.ToLower() switch
        {
            "customerid" or "id" => ascending
                ? query.OrderBy(c => c.CustomerId)
                : query.OrderByDescending(c => c.CustomerId),
            "email" => ascending
                ? query.OrderBy(c => c.Email).ThenBy(c => c.Name)
                : query.OrderByDescending(c => c.Email).ThenBy(c => c.Name),
            "city" => ascending
                ? query.OrderBy(c => c.City).ThenBy(c => c.Name)
                : query.OrderByDescending(c => c.City).ThenBy(c => c.Name),
            _ => ascending
                ? query.OrderBy(c => c.Name).ThenBy(c => c.Email)
                : query.OrderByDescending(c => c.Name).ThenBy(c => c.Email)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Orders)
            .ToListAsync();

        return (items, totalCount, totalPages);
    }

    // UPDATE
    public async Task<(bool success, string message)> UpdateAsync(
        int id,
        string name,
        string email,
        string? city)
    {

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Name is required.");

        if (name.Length > 100)
            return (false, "Name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required.");

        if (!email.Contains('@') || !email.Contains('.'))
            return (false, "Invalid email address.");

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return (false, "Customer not found.");
        
        var normalizedEmail = email.Trim().ToLower();

        if (await _context.Customers.AnyAsync(c => c.Email == normalizedEmail && c.CustomerId != id))
            return (false, "Another customer with this email already exists.");

        customer.Name = name.Trim();
        customer.Email = normalizedEmail;
        if (HashingService.IsValidBase64Salt(customer.PasswordSalt))
            customer.EmailHash = HashingService.HashValue(normalizedEmail, customer.PasswordSalt);
        customer.City = city?.Trim();

        await _context.SaveChangesAsync();
        return (true, "Customer updated.");
    }

    // UPDATE Password
    public async Task<(bool success, string message)> UpdatePasswordAsync(
        int id,
        string currentPassword,
        string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return (false, "New password is required.");

        if (newPassword.Length < 8)
            return (false, "Password must be at least 8 characters.");

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return (false, "Customer not found.");
        
        if (!HashingService.Verify(currentPassword, customer.PasswordSalt, customer.PasswordHash))
            return (false, "Incorrect current password.");
        
        var (passwordHash, passwordSalt) = HashingService.HashPassword(newPassword);
        customer.PasswordHash = passwordHash;
        customer.PasswordSalt = passwordSalt;
        customer.EmailHash = HashingService.HashValue(customer.Email, passwordSalt);

        await _context.SaveChangesAsync();
        return (true, "Password updated.");
    }

    // DELETE
    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
            return (false, "Customer not found.");
        
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return (true, "Customer and associated orders deleted.");
    }

    // Verify password
    public async Task<(bool success, Customer? customer)> VerifyPasswordAsync(string email, string password)
    {
        var customers = await _context.Customers.ToListAsync();
        var customer = customers.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());
    
        if (customer == null)
            return (false, null);

        var isValid = HashingService.Verify(password, customer.PasswordSalt, customer.PasswordHash);
        return (isValid, isValid ? customer : null);
    }

    // Get customer order summary from view
    public async Task<List<CustomerOrderSummaryView>> GetCustomerOrderSummaryAsync()
    {
        return await _context.Database
            .SqlQueryRaw<CustomerOrderSummaryView>("SELECT * FROM vw_CustomerOrderSummary")
            .ToListAsync();
    }
}
