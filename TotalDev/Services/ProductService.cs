using Microsoft.EntityFrameworkCore;
using TotalDev.Data;
using TotalDev.Models;

namespace TotalDev.Services;

public class ProductService(AppDbContext db)
{
    public Task<List<Product>> GetAllAsync() =>
        db.Products.AsNoTracking().OrderBy(p => p.ItemCode).ToListAsync();

    public Task<Product?> GetByIdAsync(int id) =>
        db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ItemCode == id);

    public Task<bool> ItemCodeExistsAsync(int itemCode, string? excludeId = null) =>
        db.Products.AnyAsync(p => p.ItemCode == itemCode && (excludeId == null || p.Id != excludeId));

    public async Task CreateAsync(Product product)
    {
        product.Id = Guid.NewGuid().ToString("N");
        product.ItemCode = await GetNextItemCodeAsync();
        product.createdOn = DateTime.UtcNow;
        product.modifyOn = DateTime.UtcNow;
        await db.Products.AddAsync(product);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Id)) return false;
        var existing = await db.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing is null) return false;

        existing.ItemCode = product.ItemCode;
        existing.ItemName = product.ItemName;
        existing.Unit = product.Unit;
        existing.Qty = product.Qty;
        existing.MRP = product.MRP;
        existing.Remarks = product.Remarks;
        existing.IsActive = product.IsActive;
        existing.modifyby = product.modifyby;
        existing.modifyOn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return false;
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetNextItemCodeAsync()
    {
        var max = await db.Products.Select(p => (int?)p.ItemCode).MaxAsync() ?? 0;
        return max + 1;
    }
}
