# E-Commerce Admin (Console Application)

A .NET 8 console application for managing an e-commerce database. Full CRUD operations for categories, products, customers, and orders. Uses Entity Framework Core with SQLite, featuring password hashing (PBKDF2), email encryption (XOR), database triggers, and SQL views.

**Language:** Swedish seed data, English code/UI

## Quick Start

**Prerequisites:** .NET 8 SDK

```bash
git clone <your-repo-url>
cd "DATABASPROJEKT - E-HANDEL ADMIN"
```

### Database Setup (Required)

Before running the application, you must create and apply migrations:

```bash
# Delete existing migrations folder if present
rm -rf Migrations

# Create initial migration (tables and seed data)
dotnet ef migrations add InitialCreate

# Create second migration for triggers and views
dotnet ef migrations add AddTriggersAndViews
```

**Important:** After creating `AddTriggersAndViews`, you must manually add the trigger and views. Open `Migrations/*_AddTriggersAndViews.cs` and replace its contents with:

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATABASPROJEKT___E_HANDEL_ADMIN.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggersAndViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Automatically update TotalAmount when OrderRow is inserted
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_UpdateOrderTotal
                AFTER INSERT ON OrderRows
                BEGIN
                    UPDATE Orders
                    SET TotalAmount = (
                        SELECT COALESCE(SUM(Quantity * UnitPrice), 0)
                        FROM OrderRows
                        WHERE OrderId = NEW.OrderId
                    )
                    WHERE OrderId = NEW.OrderId;
                END;
            ");

            // Product summary with category info
            migrationBuilder.Sql(@"
                CREATE VIEW vw_ProductSummary AS
                SELECT
                    p.ProductId,
                    p.Name AS ProductName,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    c.Name AS CategoryName,
                    p.Price * p.StockQuantity AS TotalInventoryValue
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId;
            ");

            // Customer order summary
            migrationBuilder.Sql(@"
                CREATE VIEW vw_CustomerOrderSummary AS
                SELECT
                    c.CustomerId,
                    c.Name AS CustomerName,
                    c.Email,
                    c.City,
                    COUNT(o.OrderId) AS TotalOrders,
                    COALESCE(SUM(o.TotalAmount), 0) AS TotalSpent
                FROM Customers c
                LEFT JOIN Orders o ON c.CustomerId = o.CustomerId
                GROUP BY c.CustomerId, c.Name, c.Email, c.City;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop trigger
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_UpdateOrderTotal;");

            // Drop views
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_ProductSummary;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_CustomerOrderSummary;");
        }
    }
}
```

Then apply migrations:

```bash
dotnet ef database update
```

### Run the Application

```bash
dotnet run
```

## Features

### Categories
- List with pagination and sorting
- Create, update, delete
- Cascade protection (cannot delete category with products)

### Products
- List with pagination and sorting by name, price, stock, category
- Create, update, delete
- Stock management (increment/decrement)
- Linked to categories

### Customers
- List with pagination and sorting
- Create with hashed password (PBKDF2-SHA256)
- Email encryption (XOR + Base64)
- Update profile, change password
- Password verification for login simulation
- Cascade delete (removes associated orders)

### Orders
- List with pagination and sorting by date, amount, status, customer
- Create order with multiple products (auto-deducts stock)
- Order statuses: Received, Processing, Shipped, Delivered, Cancelled
- Add/update/delete order rows
- Delete order (restores stock)
- Order statistics (total orders, revenue, pending)

### Views & Reports
- **Product Summary:** All products with category and total inventory value
- **Customer Order Summary:** All customers with order count and total spent

## Database Schema

```
Categories (1) ──────< Products (M)
                          │
Customers (1) ──────< Orders (M) ──────< OrderRows (M) >────── Products
```

**Tables:**
- `Categories` - ProductId, Name, Description
- `Products` - ProductId, Name, Description, Price, StockQuantity, CategoryId
- `Customers` - CustomerId, Name, Email (encrypted), EmailHash, City, PasswordHash, PasswordSalt
- `Orders` - OrderId, CustomerId, OrderDate, Status, TotalAmount
- `OrderRows` - OrderRowId, OrderId, ProductId, Quantity, UnitPrice

**Trigger:**
- `trg_UpdateOrderTotal` - Automatically recalculates order total when rows are inserted

**Views:**
- `vw_ProductSummary` - Products with category name and inventory value
- `vw_CustomerOrderSummary` - Customers with order statistics

## Security Features

### Password Hashing
Passwords are hashed using PBKDF2 with SHA-256:
- 100,000 iterations
- 16-byte random salt
- 32-byte hash output

### Email Encryption
Customer emails are encrypted at rest using XOR cipher with Base64 encoding. Decryption happens automatically when reading.

### Email Hashing
A separate email hash (using the password salt) allows for secure email lookups without exposing the plaintext.

## Seed Data

The database comes pre-seeded with:
- 5 categories (Elektronik, Kläder, Hem & Trädgård, Sport & Fritid, Böcker)
- 10 products across categories
- 3 demo customers (with demo passwords - not secure for production)
- 3 sample orders with order rows

**Note:** Seeded customers have placeholder password hashes. Use "Change password" to set real passwords.

## Project Structure

```
├── Program.cs              # Main application with menus
├── AppDbContext.cs         # EF Core context and seed data
├── Schemas/
│   ├── Category.cs
│   ├── Product.cs
│   ├── Customer.cs         # Includes email encryption property
│   ├── Order.cs
│   ├── OrderRow.cs
│   ├── ProductSummaryView.cs
│   └── CustomerOrderSummaryView.cs
├── Services/
│   ├── CategoryService.cs
│   ├── ProductService.cs
│   ├── CustomerService.cs
│   ├── OrderService.cs
│   ├── HashingService.cs   # PBKDF2 password hashing
│   └── EncryptionService.cs # XOR email encryption
└── Migrations/
    ├── *_InitialCreate.cs
    └── *_AddTriggersAndViews.cs
```

## Technical Stack

- **.NET 8** - Target framework
- **Entity Framework Core 9.0** - ORM
- **SQLite** - Database (file: `shop.db`)
- **PBKDF2-SHA256** - Password hashing
- **XOR Cipher** - Email encryption

## Limitations

- Console application only (no web UI)
- Single-user (no authentication layer)
- XOR encryption is not cryptographically secure (demo purposes)
- Seeded customers have invalid password salts (must reset password before use)
- SQLite limitations (no stored procedures, limited trigger support)
