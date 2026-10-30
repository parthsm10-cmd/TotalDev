using Microsoft.EntityFrameworkCore;
using TotalDev.Models;

namespace TotalDev.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order_HDR> Orders => Set<Order_HDR>();
    public DbSet<Order_Details> OrderDetails => Set<Order_Details>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.UserCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.UserCode).IsUnique();
            entity.Property(x => x.UserName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.Password).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(50).ValueGeneratedNever();
            entity.Property(x => x.CustName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.Password).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(50).ValueGeneratedNever();
            entity.HasIndex(x => x.ItemCode).IsUnique();
            entity.Property(x => x.ItemName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Qty).HasPrecision(18, 3);
            entity.Property(x => x.MRP).HasPrecision(18, 2);
            entity.Property(x => x.Remarks).HasMaxLength(500);
        });

        modelBuilder.Entity<Order_HDR>(entity =>
        {
            entity.ToTable("Order_HDR");
            entity.HasKey(x => x.Ord_No);
            entity.Property(x => x.Ord_No).ValueGeneratedOnAdd();
            entity.Property(x => x.CustomerId).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TotalQty).HasPrecision(18, 3);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId)
                .HasPrincipalKey(x => x.Id)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.Ref_Ord_No)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order_Details>(entity =>
        {
            entity.ToTable("Order_Details");
            entity.HasKey(x => x.V_No);
            entity.Property(x => x.V_No).ValueGeneratedOnAdd();
            entity.Property(x => x.Unit).HasMaxLength(20);
            entity.Property(x => x.Qty).HasPrecision(18, 3);
            entity.Property(x => x.MRP).HasPrecision(18, 2);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CustomerId).HasMaxLength(50);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.Item_Id)
                .HasPrincipalKey(x => x.ItemCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
