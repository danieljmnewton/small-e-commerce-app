using DATABASPROJEKT___E_HANDEL_ADMIN.Schemas;
using Microsoft.EntityFrameworkCore;

namespace DATABASPROJEKT___E_HANDEL_ADMIN;

public class AppDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<OrderRow> OrderRows => Set<OrderRow>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "shop.db");
        optionsBuilder.UseSqlite($"Filename={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasMany(x => x.Products)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(x => x.Email).HasField("_email");

            e.HasMany(x => x.Orders)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>()
            .HasMany(x => x.OrderRows)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderRow>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Elektronik", Description = "Elektroniska produkter och tillbehör" },
            new Category { CategoryId = 2, Name = "Kläder", Description = "Mode och kläder för alla" },
            new Category { CategoryId = 3, Name = "Hem & Trädgård", Description = "Produkter för hemmet och trädgården" },
            new Category { CategoryId = 4, Name = "Sport & Fritid", Description = "Sportartiklar och fritidsutrustning" },
            new Category { CategoryId = 5, Name = "Böcker", Description = "Böcker och litteratur" }
        );
        
        modelBuilder.Entity<Product>().HasData(
            new Product { ProductId = 1, CategoryId = 1, Name = "Laptop", Description = "Kraftfull bärbar dator", Price = 12999.00m, StockQuantity = 25 },
            new Product { ProductId = 2, CategoryId = 1, Name = "Smartphone", Description = "Senaste modellen smartphone", Price = 8999.00m, StockQuantity = 50 },
            new Product { ProductId = 3, CategoryId = 1, Name = "Hörlurar", Description = "Trådlösa hörlurar med brusreducering", Price = 2499.00m, StockQuantity = 100 },
            new Product { ProductId = 4, CategoryId = 2, Name = "T-shirt", Description = "Bekväm bomulls t-shirt", Price = 299.00m, StockQuantity = 200 },
            new Product { ProductId = 5, CategoryId = 2, Name = "Jeans", Description = "Klassiska blå jeans", Price = 699.00m, StockQuantity = 150 },
            new Product { ProductId = 6, CategoryId = 3, Name = "Trädgårdsstol", Description = "Bekväm utomhusstol", Price = 499.00m, StockQuantity = 75 },
            new Product { ProductId = 7, CategoryId = 3, Name = "Lampa", Description = "LED-bordslampa", Price = 399.00m, StockQuantity = 60 },
            new Product { ProductId = 8, CategoryId = 4, Name = "Fotboll", Description = "Officiell matchboll", Price = 349.00m, StockQuantity = 80 },
            new Product { ProductId = 9, CategoryId = 4, Name = "Yogamatta", Description = "Halkfri yogamatta", Price = 299.00m, StockQuantity = 120 },
            new Product { ProductId = 10, CategoryId = 5, Name = "Roman", Description = "Populär skönlitteratur", Price = 199.00m, StockQuantity = 300 }
        );
        
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                CustomerId = 1,
                Name = "Anna Andersson",
                Email = "anna@newton.com",
                EmailHash = "hashed_anna@newton.com",
                City = "Stockholm",
                PasswordHash = "DemoHashedPassword1",
                PasswordSalt = "DemoSalt1"
            },
            new Customer
            {
                CustomerId = 2,
                Name = "Erik Eriksson",
                Email = "erik@gmail.com",
                EmailHash = "hashed_erik@gmail.com",
                City = "Göteborg",
                PasswordHash = "DemoHashedPassword2",
                PasswordSalt = "DemoSalt2"
            },
            new Customer
            {
                CustomerId = 3,
                Name = "Maria Svensson",
                Email = "maria@outlook.com",
                EmailHash = "hashed_maria@outlook.com",
                City = "Malmö",
                PasswordHash = "DemoHashedPassword3",
                PasswordSalt = "DemoSalt3"
            }
        );
        
        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                OrderId = 1,
                CustomerId = 1,
                OrderDate = new DateTime(2024, 1, 15),
                Status = "Delivered",
                TotalAmount = 15498.00m
            },
            new Order
            {
                OrderId = 2,
                CustomerId = 2,
                OrderDate = new DateTime(2024, 2, 20),
                Status = "Shipped",
                TotalAmount = 998.00m
            },
            new Order
            {
                OrderId = 3,
                CustomerId = 1,
                OrderDate = new DateTime(2024, 3, 10),
                Status = "Processing",
                TotalAmount = 2499.00m
            }
        );
        
        modelBuilder.Entity<OrderRow>().HasData(
            new OrderRow { OrderRowId = 1, OrderId = 1, ProductId = 1, Quantity = 1, UnitPrice = 12999.00m },
            new OrderRow { OrderRowId = 2, OrderId = 1, ProductId = 3, Quantity = 1, UnitPrice = 2499.00m },
            new OrderRow { OrderRowId = 3, OrderId = 2, ProductId = 4, Quantity = 2, UnitPrice = 299.00m },
            new OrderRow { OrderRowId = 4, OrderId = 2, ProductId = 10, Quantity = 2, UnitPrice = 199.00m },
            new OrderRow { OrderRowId = 5, OrderId = 3, ProductId = 3, Quantity = 1, UnitPrice = 2499.00m }
        );
    }
}
