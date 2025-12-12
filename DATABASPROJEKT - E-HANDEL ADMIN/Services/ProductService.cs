using DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;
using Microsoft.EntityFrameworkCore;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Services;

public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task<(bool success, string message, Product? product)> CreateAsync(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        int categoryId)
    {
        
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Product name is required.", null);

        if (name.Length > 150)
            return (false, "Product name cannot exceed 150 characters.", null);

        if (string.IsNullOrWhiteSpace(description))
            return (false, "Description is required.", null);

        if (description.Length > 250)
            return (false, "Description cannot exceed 250 characters.", null);

        if (price < 0)
            return (false, "Price cannot be negative.", null);

        if (stockQuantity < 0)
            return (false, "Stock quantity cannot be negative.", null);

        if (!await _context.Categories.AnyAsync(c => c.CategoryId == categoryId))
            return (false, "Category not found.", null);

        var product = new Product
        {
            Name = name.Trim(),
            Description = description.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return (true, "Product created.", product);
    }

    // READ - Get by ID
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    // READ - Get all with pagination and sorting
    public async Task<(List<Product> items, int totalCount, int totalPages)> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string sortBy = "Name",
        bool ascending = true)
    {
        var query = _context.Products.AsQueryable();
        
        query = sortBy.ToLower() switch
        {
            "productid" or "id" => ascending
                ? query.OrderBy(p => p.ProductId)
                : query.OrderByDescending(p => p.ProductId),
            "price" => ascending
                ? query.OrderBy(p => (double)p.Price).ThenBy(p => p.Name)
                : query.OrderByDescending(p => (double)p.Price).ThenBy(p => p.Name),
            "stockquantity" or "stock" => ascending
                ? query.OrderBy(p => p.StockQuantity).ThenBy(p => p.Name)
                : query.OrderByDescending(p => p.StockQuantity).ThenBy(p => p.Name),
            "category" => ascending
                ? query.OrderBy(p => p.Category!.Name).ThenBy(p => p.Name)
                : query.OrderByDescending(p => p.Category!.Name).ThenBy(p => p.Name),
            _ => ascending
                ? query.OrderBy(p => p.Name).ThenBy(p => (double)p.Price)
                : query.OrderByDescending(p => p.Name).ThenBy(p => (double)p.Price)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Category)
            .ToListAsync();

        return (items, totalCount, totalPages);
    }

    // UPDATE
    public async Task<(bool success, string message)> UpdateAsync(
        int id,
        string name,
        string description,
        decimal price,
        int stockQuantity,
        int categoryId)
    {
        
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Product name is required.");

        if (name.Length > 150)
            return (false, "Product name cannot exceed 150 characters.");

        if (string.IsNullOrWhiteSpace(description))
            return (false, "Description is required.");

        if (description.Length > 250)
            return (false, "Description cannot exceed 250 characters.");

        if (price < 0)
            return (false, "Price cannot be negative.");

        if (stockQuantity < 0)
            return (false, "Stock quantity cannot be negative.");

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return (false, "Product not found.");

        if (!await _context.Categories.AnyAsync(c => c.CategoryId == categoryId))
            return (false, "Category not found.");

        product.Name = name.Trim();
        product.Description = description.Trim();
        product.Price = price;
        product.StockQuantity = stockQuantity;
        product.CategoryId = categoryId;

        await _context.SaveChangesAsync();
        return (true, "Product updated.");
    }

    // DELETE
    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return (false, "Product not found.");
        
        if (await _context.OrderRows.AnyAsync(or => or.ProductId == id))
            return (false, "Cannot delete product that has order rows linked.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return (true, "Product deleted.");
    }

    // Update stock quantity
    public async Task<(bool success, string message)> UpdateStockAsync(int id, int quantityChange)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return (false, "Product not found.");

        var previous = product.StockQuantity;

        var newStock = previous + quantityChange;
        if (newStock < 0)
            return (false, $"Insufficient stock. Available: {previous}");

        product.StockQuantity = newStock;
        await _context.SaveChangesAsync();

        return (true, $"Stock updated. Previous quantity: {previous}. New quantity: {newStock}");
    }

    // Get product summary from view
    public async Task<List<ProductSummaryView>> GetProductSummaryAsync()
    {
        return await _context.Database
            .SqlQueryRaw<ProductSummaryView>("SELECT * FROM vw_ProductSummary")
            .ToListAsync();
    }
}
