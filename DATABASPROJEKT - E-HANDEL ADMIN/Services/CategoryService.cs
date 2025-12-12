using DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;
using Microsoft.EntityFrameworkCore;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Services;

public class CategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    // CREATE
    public async Task<(bool success, string message, Category? category)> CreateAsync(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Category name is required.", null);

        if (name.Length > 100)
            return (false, "Category name cannot exceed 100 characters.", null);

        if (await _context.Categories.AnyAsync(c => c.Name == name))
            return (false, "A category with this name already exists.", null);

        var category = new Category
        {
            Name = name.Trim(),
            Description = description?.Trim()
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return (true, "Category created.", category);
    }

    // READ - Get by ID
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    // READ - Get all with pagination and sorting
    public async Task<(List<Category> items, int totalCount, int totalPages)> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string sortBy = "Name",
        bool ascending = true)
    {
        var query = _context.Categories.AsQueryable();
        
        query = sortBy.ToLower() switch
        {
            "categoryid" or "id" => ascending
                ? query.OrderBy(c => c.CategoryId)
                : query.OrderByDescending(c => c.CategoryId),
            "description" => ascending
                ? query.OrderBy(c => c.Description)
                : query.OrderByDescending(c => c.Description),
            _ => ascending
                ? query.OrderBy(c => c.Name)
                : query.OrderByDescending(c => c.Name)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Products)
            .ToListAsync();

        return (items, totalCount, totalPages);
    }

    // UPDATE
    public async Task<(bool success, string message)> UpdateAsync(int id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Category name is required.");

        if (name.Length > 100)
            return (false, "Category name cannot exceed 100 characters.");

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return (false, "Category not found.");

        if (await _context.Categories.AnyAsync(c => c.Name == name && c.CategoryId != id))
            return (false, "Another category with this name already exists.");

        category.Name = name.Trim();
        category.Description = description?.Trim();

        await _context.SaveChangesAsync();
        return (true, "Category updated.");
    }

    // DELETE
    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
            return (false, "Category not found.");

        if (category.Products.Count != 0)
            return (false, $"Cannot delete category. It has {category.Products.Count} products linked.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return (true, "Category deleted.");
    }
}
