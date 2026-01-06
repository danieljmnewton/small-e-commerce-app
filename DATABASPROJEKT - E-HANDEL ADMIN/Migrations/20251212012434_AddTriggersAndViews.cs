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
                CREATE TRIGGER trg_UpdateOrderTotal_Insert
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

            // Automatically update TotalAmount when OrderRow is updated
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_UpdateOrderTotal_Update
                AFTER UPDATE ON OrderRows
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

            // Automatically update TotalAmount when OrderRow is deleted
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_UpdateOrderTotal_Delete
                AFTER DELETE ON OrderRows
                BEGIN
                    UPDATE Orders
                    SET TotalAmount = (
                        SELECT COALESCE(SUM(Quantity * UnitPrice), 0)
                        FROM OrderRows
                        WHERE OrderId = OLD.OrderId
                    )
                    WHERE OrderId = OLD.OrderId;
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
            // Drop triggers
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_UpdateOrderTotal_Insert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_UpdateOrderTotal_Update;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_UpdateOrderTotal_Delete;");

            // Drop views
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_ProductSummary;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_CustomerOrderSummary;");
        }
    }
}
