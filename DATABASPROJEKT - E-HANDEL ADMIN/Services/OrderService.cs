using DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;
using Microsoft.EntityFrameworkCore;

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Services;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    // CREATE Order with transaction and stock update
    public async Task<(bool success, string message, Order? order)> CreateOrderAsync(
        int customerId,
        List<(int productId, int quantity)> items)
    {
        if (!items.Any())
            return (false, "Order must contain at least one product.", null);
        
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (!await _context.Customers.AnyAsync(c => c.CustomerId == customerId))
            {
                await transaction.RollbackAsync();
                return (false, "Customer not found.", null);
            }
            
            var productIds = items.Select(i => i.productId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            if (products.Count != productIds.Distinct().Count())
            {
                await transaction.RollbackAsync();
                return (false, "One or more products not found.", null);
            }
            
            foreach (var (productId, quantity) in items)
            {
                var product = products.First(p => p.ProductId == productId);
                if (product.StockQuantity < quantity)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}", null);
                }
            }
            
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = "Received",
                TotalAmount = 0
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var (productId, quantity) in items)
            {
                var product = products.First(p => p.ProductId == productId);

                var orderRow = new OrderRow
                {
                    OrderId = order.OrderId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };

                _context.OrderRows.Add(orderRow);

                product.StockQuantity -= quantity;
            }

            await _context.SaveChangesAsync();

            // Reload order to get trigger-calculated TotalAmount
            await _context.Entry(order).ReloadAsync();

            await transaction.CommitAsync();

            return (true, $"Order #{order.OrderId} created with total amount {order.TotalAmount:C}.", order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error creating order: {ex.Message}", null);
        }
    }

    // READ - Get by ID
    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderRows)
                .ThenInclude(or => or.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    // READ - Get all with pagination and sorting
    public async Task<(List<Order> items, int totalCount, int totalPages)> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string sortBy = "OrderDate",
        bool ascending = false,
        string? status = null,
        int? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.Orders.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }
        
        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }
        
        if (fromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= toDate.Value);
        }
        
        query = sortBy.ToLower() switch
        {
            "orderid" or "id" => ascending
                ? query.OrderBy(o => o.OrderId)
                : query.OrderByDescending(o => o.OrderId),
            "totalamount" or "total" => ascending
                ? query.OrderBy(o => o.TotalAmount).ThenByDescending(o => o.OrderDate)
                : query.OrderByDescending(o => o.TotalAmount).ThenByDescending(o => o.OrderDate),
            "status" => ascending
                ? query.OrderBy(o => o.Status).ThenByDescending(o => o.OrderDate)
                : query.OrderByDescending(o => o.Status).ThenByDescending(o => o.OrderDate),
            "customer" => ascending
                ? query.OrderBy(o => o.Customer!.Name).ThenByDescending(o => o.OrderDate)
                : query.OrderByDescending(o => o.Customer!.Name).ThenByDescending(o => o.OrderDate),
            _ => ascending
                ? query.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderId)
                : query.OrderByDescending(o => o.OrderDate).ThenBy(o => o.OrderId)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.Customer)
            .Include(o => o.OrderRows)
                .ThenInclude(or => or.Product)
            .ToListAsync();

        return (items, totalCount, totalPages);
    }

    // UPDATE Status
    public async Task<(bool success, string message)> UpdateStatusAsync(int id, string newStatus)
    {
        var validStatuses = new[] { "Received", "Processing", "Shipped", "Delivered", "Cancelled" };

        if (string.IsNullOrWhiteSpace(newStatus))
            return (false, "Status is required.");

        if (!validStatuses.Contains(newStatus))
            return (false, $"Invalid status. Valid values: {string.Join(", ", validStatuses)}");

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return (false, "Order not found.");

        order.Status = newStatus;
        await _context.SaveChangesAsync();
        return (true, $"Order status updated to '{newStatus}'.");
    }

    // DELETE Order with transaction (restores stock)
    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderRows)
                    .ThenInclude(or => or.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                await transaction.RollbackAsync();
                return (false, "Order not found.");
            }
            
            foreach (var row in order.OrderRows)
            {
                if (row.Product != null)
                {
                    row.Product.StockQuantity += row.Quantity;
                }
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Order deleted and stock restored.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error deleting order: {ex.Message}");
        }
    }

    // Add OrderRow with transaction
    public async Task<(bool success, string message)> AddOrderRowAsync(int orderId, int productId, int quantity)
    {
        if (quantity <= 0)
            return (false, "Quantity must be greater than 0.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                await transaction.RollbackAsync();
                return (false, "Order not found.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                await transaction.RollbackAsync();
                return (false, "Product not found.");
            }

            if (product.StockQuantity < quantity)
            {
                await transaction.RollbackAsync();
                return (false, $"Insufficient stock. Available: {product.StockQuantity}");
            }

            var orderRow = new OrderRow
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };

            _context.OrderRows.Add(orderRow);
            product.StockQuantity -= quantity;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Order row added.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error adding order row: {ex.Message}");
        }
    }

    // Update OrderRow quantity with transaction
    public async Task<(bool success, string message)> UpdateOrderRowQuantityAsync(int orderRowId, int newQuantity)
    {
        if (newQuantity <= 0)
            return (false, "Quantity must be greater than 0.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderRow = await _context.OrderRows
                .Include(or => or.Product)
                .Include(or => or.Order)
                .FirstOrDefaultAsync(or => or.OrderRowId == orderRowId);

            if (orderRow == null)
            {
                await transaction.RollbackAsync();
                return (false, "Order row not found.");
            }

            var quantityDiff = newQuantity - orderRow.Quantity;

            if (orderRow.Product!.StockQuantity < quantityDiff)
            {
                await transaction.RollbackAsync();
                return (false, $"Insufficient stock. Available: {orderRow.Product.StockQuantity}");
            }

            orderRow.Product.StockQuantity -= quantityDiff;

            orderRow.Quantity = newQuantity;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Order row updated.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error updating order row: {ex.Message}");
        }
    }

    // Delete OrderRow with transaction (restores stock)
    public async Task<(bool success, string message)> DeleteOrderRowAsync(int orderRowId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderRow = await _context.OrderRows
                .Include(or => or.Product)
                .Include(or => or.Order)
                .FirstOrDefaultAsync(or => or.OrderRowId == orderRowId);

            if (orderRow == null)
            {
                await transaction.RollbackAsync();
                return (false, "Order row not found.");
            }

            orderRow.Product!.StockQuantity += orderRow.Quantity;

            _context.OrderRows.Remove(orderRow);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Order row deleted and stock restored.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error deleting order row: {ex.Message}");
        }
    }

    // Get order statistics
    public async Task<(int totalOrders, decimal totalRevenue, int pendingOrders)> GetStatisticsAsync()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
        var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Received" || o.Status == "Processing");

        return (totalOrders, totalRevenue, pendingOrders);
    }
}
