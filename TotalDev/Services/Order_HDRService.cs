
using Microsoft.EntityFrameworkCore;
using TotalDev.Data;
using TotalDev.Models;

namespace TotalDev.Services;

public class Order_HDRService(AppDbContext db)
{
    // CREATE ORDER
    public async Task CreateOrderAsync(Order_HDR header, List<Order_Details> details)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync();

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

    // GET ALL ORDERS
    public Task<List<Order_HDR>> GetAllAsync()
    {
        return db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.Ord_No)
            .ToListAsync();
    }

    // GET ORDER FOR EDIT USING ORDER NO AND CUSTOMER ID
    public async Task<Order_HDR?> GetOrderForEditAsync(
        int ordNo,
        int customerId)
    {
        return await db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o =>
                o.Ord_No == ordNo &&
                o.CustomerId == customerId);
    }

    // GET ORDER DETAILS
    public async Task<List<Order_Details>> GetOrderDetailsAsync(int ordNo, int Cust_Id)
    {
        return await db.OrderDetails
            .AsNoTracking()
            .Include(d => d.Product)
            .Where(d => d.Ref_Ord_No == ordNo && d.CustomerId == Cust_Id)
            .ToListAsync();
    }

    // UPDATE COMPLETE ORDER
    public async Task UpdateOrderAsync(int ordNo, int customerId, Order_HDR updatedHeader, List<Order_Details> updatedDetails)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync();

            try
            {
                // Find existing order
                var existingOrder = await db.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o =>
                        o.Ord_No == ordNo &&
                        o.CustomerId == customerId);

                if (existingOrder == null)
                    throw new Exception("Order not found.");

                // Update header
                existingOrder.Ord_Date = updatedHeader.Ord_Date;
                existingOrder.TotalQty = updatedHeader.TotalQty;
                existingOrder.TotalAmount = updatedHeader.TotalAmount;
                existingOrder.Modify_On = DateTime.UtcNow;

                // Remove old details
                db.OrderDetails.RemoveRange(existingOrder.Items);

                // Add updated details
                existingOrder.Items = updatedDetails;

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

    // Report Download Method
    public async Task<List<Order_HDR>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {

        DateTime startDate = fromDate.Date;
        DateTime endDate = toDate.Date.AddDays(1);

        return await db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Where(o =>
                o.Ord_Date >= startDate &&
                o.Ord_Date < endDate)
            .OrderBy(o => o.Ord_No)
            .ToListAsync();
    }
}


