using DATABASPROJEKT___E_HANDEL_ADMIN;
using DATABASPROJEKT___E_HANDEL_ADMIN.Services;

await using var context = new AppDbContext();

var categoryService = new CategoryService(context);
var productService = new ProductService(context);
var customerService = new CustomerService(context);
var orderService = new OrderService(context);

while (true)
{
    ShowMainMenu();
    var choice = ReadLine("Choose an option");

    switch (choice)
    {
        case "1": await CategoryMenu(); break;
        case "2": await ProductMenu(); break;
        case "3": await CustomerMenu(); break;
        case "4": await OrderMenu(); break;
        case "5": await ViewsAndReportsMenu(); break;
        case "0":
            Console.WriteLine("Exiting program...");
            return;
        default:
            Console.WriteLine("Invalid choice, try again.");
            break;
    }
}

void ShowMainMenu()
{
    Console.WriteLine("\n=== MAIN MENU ===");
    Console.WriteLine("1. Manage Categories");
    Console.WriteLine("2. Manage Products");
    Console.WriteLine("3. Manage Customers");
    Console.WriteLine("4. Manage Orders");
    Console.WriteLine("5. Views & Reports");
    Console.WriteLine("0. Exit");
}

// CATEGORY MENU
async Task CategoryMenu()
{
    while (true)
    {
        Console.WriteLine("\n=== CATEGORIES ===");
        Console.WriteLine("1. List categories");
        Console.WriteLine("2. Create category");
        Console.WriteLine("3. Update category");
        Console.WriteLine("4. Delete category");
        Console.WriteLine("0. Back");

        var choice = ReadLine("Choose");
        switch (choice)
        {
            case "1": await ListCategories(); break;
            case "2": await CreateCategory(); break;
            case "3": await UpdateCategory(); break;
            case "4": await DeleteCategory(); break;
            case "0": return;
        }
    }
}

async Task ListCategories()
{
    var page = 1;
    var pageSize = 5;
    var sortBy = "Name";
    var ascending = true;

    while (true)
    {
        var (items, totalCount, totalPages) = await categoryService.GetAllAsync(page, pageSize, sortBy, ascending);

        Console.WriteLine($"\n--- Categories (Page {page}/{totalPages}, Total: {totalCount}) ---");
        Console.WriteLine($"{"ID",-5} {"Name",-20} {"Description",-30} {"Products",-10}");
        Console.WriteLine(new string('-', 70));

        foreach (var cat in items)
        {
            Console.WriteLine($"{cat.CategoryId,-5} {cat.Name,-20} {(cat.Description ?? "-"),-30} {cat.Products.Count,-10}");
        }

        Console.WriteLine($"\n[N]ext page | [P]revious | [S]ort ({sortBy} {(ascending ? "ASC" : "DESC")}) | [Q]uit");
        var nav = ReadLine("").ToUpper();

        switch (nav)
        {
            case "N": if (page < totalPages) page++; break;
            case "P": if (page > 1) page--; break;
            case "S":
                Console.WriteLine("Sort by: 1. Name 2. ID 3. Description");
                var sortChoice = ReadLine("");
                sortBy = sortChoice switch { "2" => "Id", "3" => "Description", _ => "Name" };
                ascending = !ascending;
                break;
            case "Q": return;
        }
    }
}

async Task CreateCategory()
{
    var name = ReadLine("Name");
    var description = ReadLine("Description (optional)");

    var (success, message, _) = await categoryService.CreateAsync(name, string.IsNullOrWhiteSpace(description) ? null : description);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task UpdateCategory()
{
    if (!int.TryParse(ReadLine("Category ID"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var category = await categoryService.GetByIdAsync(id);
    if (category == null)
    {
        Console.WriteLine("Category not found.");
        return;
    }

    Console.WriteLine($"Current name: {category.Name}");
    Console.WriteLine($"Current description: {category.Description ?? "-"}");

    var name = ReadLine($"New name (enter to keep)");
    var description = ReadLine($"New description (enter to keep)");

    var (success, message) = await categoryService.UpdateAsync(
        id,
        string.IsNullOrWhiteSpace(name) ? category.Name : name,
        string.IsNullOrWhiteSpace(description) ? category.Description : description
    );
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task DeleteCategory()
{
    if (!int.TryParse(ReadLine("Category ID to delete"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var (success, message) = await categoryService.DeleteAsync(id);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

// PRODUCT MENU
async Task ProductMenu()
{
    while (true)
    {
        Console.WriteLine("\n=== PRODUCTS ===");
        Console.WriteLine("1. List products");
        Console.WriteLine("2. Create product");
        Console.WriteLine("3. Update product");
        Console.WriteLine("4. Delete product");
        Console.WriteLine("5. Update stock");
        Console.WriteLine("0. Back");

        var choice = ReadLine("Choose");
        switch (choice)
        {
            case "1": await ListProducts(); break;
            case "2": await CreateProduct(); break;
            case "3": await UpdateProduct(); break;
            case "4": await DeleteProduct(); break;
            case "5": await UpdateStock(); break;
            case "0": return;
        }
    }
}

async Task ListProducts()
{
    var page = 1;
    var pageSize = 5;
    var sortBy = "Name";
    var ascending = true;

    while (true)
    {
        var (items, totalCount, totalPages) = await productService.GetAllAsync(page, pageSize, sortBy, ascending);

        Console.WriteLine($"\n--- Products (Page {page}/{totalPages}, Total: {totalCount}) ---");
        Console.WriteLine($"{"ID",-5} {"Name",-20} {"Price",-12} {"Stock",-8} {"Category",-15}");
        Console.WriteLine(new string('-', 65));

        foreach (var prod in items)
        {
            Console.WriteLine($"{prod.ProductId,-5} {prod.Name,-20} {prod.Price,10:C} {prod.StockQuantity,-8} {prod.Category?.Name ?? "-",-15}");
        }

        Console.WriteLine($"\n[N]ext | [P]revious | [S]ort ({sortBy}) | [Q]uit");
        var nav = ReadLine("").ToUpper();

        switch (nav)
        {
            case "N": if (page < totalPages) page++; break;
            case "P": if (page > 1) page--; break;
            case "S":
                Console.WriteLine("Sort by: 1. Name 2. Price 3. Stock 4. Category");
                var sortChoice = ReadLine("");
                sortBy = sortChoice switch { "2" => "Price", "3" => "Stock", "4" => "Category", _ => "Name" };
                ascending = !ascending;
                break;
            case "Q": return;
        }
    }
}

async Task CreateProduct()
{
    var name = ReadLine("Product name");
    var description = ReadLine("Description");

    if (!decimal.TryParse(ReadLine("Price"), out var price))
    {
        Console.WriteLine("Invalid price.");
        return;
    }

    if (!int.TryParse(ReadLine("Stock quantity"), out var stock))
    {
        Console.WriteLine("Invalid stock quantity.");
        return;
    }

    // Show categories
    var (categories, _, _) = await categoryService.GetAllAsync(1, 100);
    Console.WriteLine("Available categories:");
    foreach (var cat in categories)
    {
        Console.WriteLine($"  [{cat.CategoryId}] {cat.Name}");
    }

    if (!int.TryParse(ReadLine("Category ID"), out var categoryId))
    {
        Console.WriteLine("Invalid category ID.");
        return;
    }

    var (success, message, _) = await productService.CreateAsync(name, description, price, stock, categoryId);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task UpdateProduct()
{
    if (!int.TryParse(ReadLine("Product ID"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var product = await productService.GetByIdAsync(id);
    if (product == null)
    {
        Console.WriteLine("Product not found.");
        return;
    }

    Console.WriteLine($"Current: {product.Name} | {product.Price:C} | Stock: {product.StockQuantity} | Category: {product.Category?.Name}");

    var name = ReadLine($"New name (enter to keep)");
    var description = ReadLine($"New description (enter to keep)");
    var priceStr = ReadLine($"New price (enter to keep)");
    var stockStr = ReadLine($"New stock quantity (enter to keep)");
    var categoryIdStr = ReadLine($"New category ID (enter to keep)");

    var (success, message) = await productService.UpdateAsync(
        id,
        string.IsNullOrWhiteSpace(name) ? product.Name : name,
        string.IsNullOrWhiteSpace(description) ? product.Description : description,
        string.IsNullOrWhiteSpace(priceStr) ? product.Price : decimal.Parse(priceStr),
        string.IsNullOrWhiteSpace(stockStr) ? product.StockQuantity : int.Parse(stockStr),
        string.IsNullOrWhiteSpace(categoryIdStr) ? product.CategoryId : int.Parse(categoryIdStr)
    );
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task DeleteProduct()
{
    if (!int.TryParse(ReadLine("Product ID to delete"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var (success, message) = await productService.DeleteAsync(id);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task UpdateStock()
{
    if (!int.TryParse(ReadLine("Product ID"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    if (!int.TryParse(ReadLine("Change (+/- quantity)"), out var change))
    {
        Console.WriteLine("Invalid change.");
        return;
    }

    var (success, message) = await productService.UpdateStockAsync(id, change);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

// CUSTOMER MENU
async Task CustomerMenu()
{
    while (true)
    {
        Console.WriteLine("\n=== CUSTOMERS ===");
        Console.WriteLine("1. List customers");
        Console.WriteLine("2. Create customer");
        Console.WriteLine("3. Update customer");
        Console.WriteLine("4. Delete customer");
        Console.WriteLine("5. Change password");
        Console.WriteLine("6. Verify password");
        Console.WriteLine("0. Back");

        var choice = ReadLine("Choose");
        switch (choice)
        {
            case "1": await ListCustomers(); break;
            case "2": await CreateCustomer(); break;
            case "3": await UpdateCustomer(); break;
            case "4": await DeleteCustomer(); break;
            case "5": await ChangePassword(); break;
            case "6": await VerifyPassword(); break;
            case "0": return;
        }
    }
}

async Task ListCustomers()
{
    var page = 1;
    var pageSize = 5;
    var sortBy = "Name";
    var ascending = true;

    while (true)
    {
        var (items, totalCount, totalPages) = await customerService.GetAllAsync(page, pageSize, sortBy, ascending);

        Console.WriteLine($"\n--- Customers (Page {page}/{totalPages}, Total: {totalCount}) ---");
        Console.WriteLine($"{"ID",-5} {"Name",-20} {"Email",-25} {"City",-15} {"Orders",-8}");
        Console.WriteLine(new string('-', 78));

        foreach (var cust in items)
        {
            Console.WriteLine($"{cust.CustomerId,-5} {cust.Name,-20} {cust.Email,-25} {(cust.City ?? "-"),-15} {cust.Orders.Count,-8}");
        }

        Console.WriteLine($"\n[N]ext | [P]revious | [S]ort ({sortBy}) | [Q]uit");
        var nav = ReadLine("").ToUpper();

        switch (nav)
        {
            case "N": if (page < totalPages) page++; break;
            case "P": if (page > 1) page--; break;
            case "S":
                Console.WriteLine("Sort by: 1. Name 2. Email 3. City");
                var sortChoice = ReadLine("");
                sortBy = sortChoice switch { "2" => "Email", "3" => "City", _ => "Name" };
                ascending = !ascending;
                break;
            case "Q": return;
        }
    }
}

async Task CreateCustomer()
{
    var name = ReadLine("Name");
    var email = ReadLine("Email");
    var password = ReadLine("Password (min 8 characters)");
    var city = ReadLine("City (optional)");

    var (success, message, _) = await customerService.CreateAsync(
        name, email, password,
        string.IsNullOrWhiteSpace(city) ? null : city
    );
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task UpdateCustomer()
{
    if (!int.TryParse(ReadLine("Customer ID"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var customer = await customerService.GetByIdAsync(id);
    if (customer == null)
    {
        Console.WriteLine("Customer not found.");
        return;
    }

    Console.WriteLine($"Current: {customer.Name} | {customer.Email} | {customer.City ?? "-"}");

    var name = ReadLine($"New name (enter to keep)");
    var email = ReadLine($"New email (enter to keep)");
    var city = ReadLine($"New city (enter to keep)");

    var (success, message) = await customerService.UpdateAsync(
        id,
        string.IsNullOrWhiteSpace(name) ? customer.Name : name,
        string.IsNullOrWhiteSpace(email) ? customer.Email : email,
        string.IsNullOrWhiteSpace(city) ? customer.City : city
    );
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task DeleteCustomer()
{
    if (!int.TryParse(ReadLine("Customer ID to delete"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var (success, message) = await customerService.DeleteAsync(id);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task ChangePassword()
{
    if (!int.TryParse(ReadLine("Customer ID"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var currentPassword = ReadLine("Current password");
    var newPassword = ReadLine("New password");

    var (success, message) = await customerService.UpdatePasswordAsync(id, currentPassword, newPassword);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task VerifyPassword()
{
    var email = ReadLine("Email");
    var password = ReadLine("Password");

    var (success, customer) = await customerService.VerifyPasswordAsync(email, password);
    if (success)
    {
        Console.WriteLine($"OK: Password verified for {customer!.Name}");
    }
    else
    {
        Console.WriteLine("ERROR: Invalid login credentials");
    }
}

// ORDER MENU
async Task OrderMenu()
{
    while (true)
    {
        Console.WriteLine("\n=== ORDERS ===");
        Console.WriteLine("1. List orders");
        Console.WriteLine("2. Create order");
        Console.WriteLine("3. Show order details");
        Console.WriteLine("4. Update order status");
        Console.WriteLine("5. Delete order");
        Console.WriteLine("6. Manage order rows");
        Console.WriteLine("7. Order statistics");
        Console.WriteLine("0. Back");

        var choice = ReadLine("Choose");
        switch (choice)
        {
            case "1": await ListOrders(); break;
            case "2": await CreateOrder(); break;
            case "3": await ShowOrderDetails(); break;
            case "4": await UpdateOrderStatus(); break;
            case "5": await DeleteOrder(); break;
            case "6": await ManageOrderRows(); break;
            case "7": await ShowOrderStatistics(); break;
            case "0": return;
        }
    }
}

async Task ListOrders()
{
    var page = 1;
    var pageSize = 5;
    var sortBy = "OrderDate";
    var ascending = false;

    while (true)
    {
        var (items, totalCount, totalPages) = await orderService.GetAllAsync(page, pageSize, sortBy, ascending);

        Console.WriteLine($"\n--- Orders (Page {page}/{totalPages}, Total: {totalCount}) ---");
        Console.WriteLine($"{"ID",-5} {"Date",-12} {"Customer",-20} {"Status",-12} {"Amount",-12}");
        Console.WriteLine(new string('-', 65));

        foreach (var order in items)
        {
            Console.WriteLine($"{order.OrderId,-5} {order.OrderDate:yyyy-MM-dd} {order.Customer?.Name ?? "-",-20} {order.Status,-12} {order.TotalAmount,10:C}");
        }

        Console.WriteLine($"\n[N]ext | [P]revious | [S]ort ({sortBy}) | [Q]uit");
        var nav = ReadLine("").ToUpper();

        switch (nav)
        {
            case "N": if (page < totalPages) page++; break;
            case "P": if (page > 1) page--; break;
            case "S":
                Console.WriteLine("Sort by: 1. Date 2. Amount 3. Status 4. Customer");
                var sortChoice = ReadLine("");
                sortBy = sortChoice switch { "2" => "Total", "3" => "Status", "4" => "Customer", _ => "OrderDate" };
                ascending = !ascending;
                break;
            case "Q": return;
        }
    }
}

async Task CreateOrder()
{
    var (customers, _, _) = await customerService.GetAllAsync(1, 100);
    Console.WriteLine("Available customers:");
    foreach (var cust in customers)
    {
        Console.WriteLine($"  [{cust.CustomerId}] {cust.Name}");
    }

    if (!int.TryParse(ReadLine("Customer ID"), out var customerId))
    {
        Console.WriteLine("Invalid customer ID.");
        return;
    }
    
    var (products, _, _) = await productService.GetAllAsync(1, 100);
    Console.WriteLine("\nAvailable products:");
    foreach (var prod in products)
    {
        Console.WriteLine($"  [{prod.ProductId}] {prod.Name} - {prod.Price:C} (Stock: {prod.StockQuantity})");
    }

    var items = new List<(int productId, int quantity)>();
    while (true)
    {
        var productIdStr = ReadLine("\nProduct ID (enter to finish)");
        if (string.IsNullOrWhiteSpace(productIdStr)) break;

        if (!int.TryParse(productIdStr, out var productId))
        {
            Console.WriteLine("Invalid product ID.");
            continue;
        }

        if (!int.TryParse(ReadLine("Quantity"), out var quantity) || quantity <= 0)
        {
            Console.WriteLine("Invalid quantity.");
            continue;
        }

        items.Add((productId, quantity));
        Console.WriteLine($"Added product {productId} x {quantity}");
    }

    if (items.Count == 0)
    {
        Console.WriteLine("No products selected.");
        return;
    }

    var (success, message, _) = await orderService.CreateOrderAsync(customerId, items);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task ShowOrderDetails()
{
    if (!int.TryParse(ReadLine("Order ID"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var order = await orderService.GetByIdAsync(id);
    if (order == null)
    {
        Console.WriteLine("Order not found.");
        return;
    }

    Console.WriteLine($"\n=== Order #{order.OrderId} ===");
    Console.WriteLine($"Date: {order.OrderDate:yyyy-MM-dd HH:mm}");
    Console.WriteLine($"Customer: {order.Customer?.Name} ({order.Customer?.Email})");
    Console.WriteLine($"Status: {order.Status}");
    Console.WriteLine($"\nOrder rows:");
    Console.WriteLine($"{"RowID",-8} {"Product",-20} {"Qty",-8} {"Price",-12} {"Total",-12}");
    Console.WriteLine(new string('-', 65));

    foreach (var row in order.OrderRows)
    {
        var sum = row.Quantity * row.UnitPrice;
        Console.WriteLine($"{row.OrderRowId,-8} {row.Product?.Name ?? "-",-20} {row.Quantity,-8} {row.UnitPrice,10:C} {sum,10:C}");
    }

    Console.WriteLine(new string('-', 65));
    Console.WriteLine($"{"TOTAL:",-50} {order.TotalAmount,10:C}");
}

async Task UpdateOrderStatus()
{
    if (!int.TryParse(ReadLine("Order ID"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Console.WriteLine("Available statuses: Received, Processing, Shipped, Delivered, Cancelled");
    var status = ReadLine("New status");

    var (success, message) = await orderService.UpdateStatusAsync(id, status);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task DeleteOrder()
{
    if (!int.TryParse(ReadLine("Order ID to delete"), out var id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Console.WriteLine("WARNING: This will restore stock for all products in the order.");
    var confirm = ReadLine("Are you sure? (y/n)");
    if (string.IsNullOrWhiteSpace(confirm) || confirm.Trim().ToLower() != "y")
    {
        Console.WriteLine("Cancelled.");
        return;
    }

    var (success, message) = await orderService.DeleteAsync(id);
    Console.WriteLine(success ? $"OK: {message}" : $"ERROR: {message}");
}

async Task ManageOrderRows()
{
    Console.WriteLine("\n=== MANAGE ORDER ROWS ===");
    Console.WriteLine("1. Add order row");
    Console.WriteLine("2. Update quantity");
    Console.WriteLine("3. Delete order row");
    Console.WriteLine("0. Back");

    var choice = ReadLine("Choose");
    switch (choice)
    {
        case "1":
            if (!int.TryParse(ReadLine("Order ID"), out var orderId))
            {
                Console.WriteLine("Invalid order ID.");
                return;
            }
            if (!int.TryParse(ReadLine("Product ID"), out var productId))
            {
                Console.WriteLine("Invalid product ID.");
                return;
            }
            if (!int.TryParse(ReadLine("Quantity"), out var qty) || qty <= 0)
            {
                Console.WriteLine("Invalid quantity.");
                return;
            }
            var (s1, m1) = await orderService.AddOrderRowAsync(orderId, productId, qty);
            Console.WriteLine(s1 ? $"OK: {m1}" : $"ERROR: {m1}");
            break;

        case "2":
            if (!int.TryParse(ReadLine("Order row ID"), out var rowId))
            {
                Console.WriteLine("Invalid order row ID.");
                return;
            }
            if (!int.TryParse(ReadLine("New quantity"), out var newQty) || newQty <= 0)
            {
                Console.WriteLine("Invalid quantity.");
                return;
            }
            var (s2, m2) = await orderService.UpdateOrderRowQuantityAsync(rowId, newQty);
            Console.WriteLine(s2 ? $"OK: {m2}" : $"ERROR: {m2}");
            break;

        case "3":
            if (!int.TryParse(ReadLine("Order row ID to delete"), out var delRowId))
            {
                Console.WriteLine("Invalid order row ID.");
                return;
            }
            var (s3, m3) = await orderService.DeleteOrderRowAsync(delRowId);
            Console.WriteLine(s3 ? $"OK: {m3}" : $"ERROR: {m3}");
            break;
    }
}

async Task ShowOrderStatistics()
{
    var (totalOrders, totalRevenue, pendingOrders) = await orderService.GetStatisticsAsync();

    Console.WriteLine("\n=== ORDER STATISTICS ===");
    Console.WriteLine($"Total orders: {totalOrders}");
    Console.WriteLine($"Total revenue: {totalRevenue:C}");
    Console.WriteLine($"Pending orders: {pendingOrders}");
}

// VIEWS AND REPORTS
async Task ViewsAndReportsMenu()
{
    while (true)
    {
        Console.WriteLine("\n=== VIEWS & REPORTS ===");
        Console.WriteLine("1. Product overview");
        Console.WriteLine("2. Customer order summary");
        Console.WriteLine("0. Back");

        var choice = ReadLine("Choose");
        switch (choice)
        {
            case "1": await ShowProductSummaryView(); break;
            case "2": await ShowCustomerOrderSummaryView(); break;
            case "0": return;
        }
    }
}

async Task ShowProductSummaryView()
{
    Console.WriteLine("\n=== PRODUCT OVERVIEW ===");

    try
    {
        var summary = await productService.GetProductSummaryAsync();

        Console.WriteLine($"{"ID",-5} {"Product",-20} {"Price",-12} {"Stock",-8} {"Category",-15} {"Inventory Value",-15}");
        Console.WriteLine(new string('-', 80));

        foreach (var item in summary)
        {
            Console.WriteLine($"{item.ProductId,-5} {item.ProductName,-20} {item.Price,10:C} {item.StockQuantity,-8} {item.CategoryName,-15} {item.TotalInventoryValue,13:C}");
        }

        var totalValue = summary.Sum(s => s.TotalInventoryValue);
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"TOTAL INVENTORY VALUE:",-62} {totalValue,13:C}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading view: {ex.Message}");
    }
}

async Task ShowCustomerOrderSummaryView()
{
    Console.WriteLine("\n=== CUSTOMER ORDER SUMMARY ===");

    try
    {
        var summary = await customerService.GetCustomerOrderSummaryAsync();

        Console.WriteLine($"{"ID",-5} {"Name",-20} {"Email",-25} {"City",-15} {"Orders",-8} {"Total",-12}");
        Console.WriteLine(new string('-', 90));

        foreach (var item in summary)
        {
            Console.WriteLine($"{item.CustomerId,-5} {item.CustomerName,-20} {item.Email,-25} {(item.City ?? "-"),-15} {item.TotalOrders,-8} {item.TotalSpent,10:C}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading view: {ex.Message}");
    }
}

// Helper method
string ReadLine(string prompt)
{
    if (!string.IsNullOrWhiteSpace(prompt))
    {
        Console.Write($"{prompt}: ");
    }
    return Console.ReadLine() ?? "";
}
