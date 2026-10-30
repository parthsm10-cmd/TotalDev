using Microsoft.EntityFrameworkCore;
using TotalDev.Data;
using TotalDev.Models;

namespace TotalDev.Services;

public class Order_HDRService(AppDbContext db)
{

    public async Task CreateOrderAsync(
        Order_HDR header,
        List<Order_Details> details)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                header.CreatedOn = DateTime.UtcNow;
                header.Modify_On = DateTime.UtcNow;
                header.Items = details;
                await db.Orders.AddAsync(header);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }


    public Task<List<Order_HDR>> GetAllAsync() =>
        db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.Ord_No)
            .ToListAsync();
}
