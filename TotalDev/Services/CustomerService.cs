using Microsoft.EntityFrameworkCore;
using TotalDev.Data;
using TotalDev.Models;

namespace TotalDev.Services;

public class CustomerService(AppDbContext db)
{
    public Task<List<Customer>> GetAllAsync() =>
        db.Customers.AsNoTracking().OrderBy(c => c.CustName).ToListAsync();

    public Task<Customer?> GetByIdAsync(int id) =>
        db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task CreateAsync(Customer customer)
    {
        customer.Id = await GetNextCustomerCodeAsync();
        customer.createdOn = DateTime.UtcNow;
        customer.modifyOn = DateTime.UtcNow;
        await db.Customers.AddAsync(customer);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        if (customer.Id == 0) return false;
        var existing = await db.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        if (existing is null) return false;

        existing.CustName = customer.CustName;
        existing.Mobile = customer.Mobile;
        existing.City = customer.City;
        existing.Address = customer.Address;
        existing.Password = customer.Password;
        existing.IsActive = customer.IsActive;
        existing.modifyby = customer.modifyby;
        existing.modifyOn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return false;
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return true;
    }
    public async Task<int> GetNextCustomerCodeAsync()
    {
        var max = await db.Customers.Select(c => (int?)c.Id).MaxAsync() ?? 0;
        return max + 1;
    }
}
