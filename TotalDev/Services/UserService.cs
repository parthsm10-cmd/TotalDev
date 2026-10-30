using Microsoft.EntityFrameworkCore;
using TotalDev.Data;
using TotalDev.Models;

namespace TotalDev.Services;

public class UserService(AppDbContext db)
{
    public Task<List<User>> GetAllAsync() =>
        db.Users.AsNoTracking().OrderBy(u => u.UserCode).ToListAsync();

    public Task<User?> GetByIdAsync(string id) =>
        db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public Task<bool> UserCodeExistsAsync(string userCode, string? excludeId = null) =>
        db.Users.AnyAsync(u => u.UserCode == userCode && (excludeId == null || u.Id != excludeId));

    public async Task CreateAsync(User user)
    {
        user.Id = Guid.NewGuid().ToString("N");
        user.UserCode = user.UserCode.Trim();
        user.createdOn = DateTime.UtcNow;
        user.modifyOn = DateTime.UtcNow;
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Id)) return false;
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existing is null) return false;

        existing.UserCode = user.UserCode.Trim();
        existing.UserName = user.UserName;
        existing.Mobile = user.Mobile;
        existing.City = user.City;
        existing.Address = user.Address;
        existing.Password = user.Password;
        existing.IsActive = user.IsActive;
        existing.modifyby = user.modifyby;
        existing.modifyOn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<string> GetNextUserCodeAsync()
    {
        var codes = await db.Users.Select(u => u.UserCode).ToListAsync();
        var max = codes.Select(c => int.TryParse(c, out var value) ? value : 0).DefaultIfEmpty(0).Max();
        return (max + 1).ToString();
    }
}
