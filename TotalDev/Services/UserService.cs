using MongoDB.Bson;
using MongoDB.Driver;
using TotalDev.Models;

namespace TotalDev.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(
        IMongoClient client,
        IConfiguration configuration)
    {
        var databaseName =
            configuration["MongoDbSettings:DatabaseName"]
            ?? throw new InvalidOperationException(
                "Database name missing.");

        var collectionName =
            configuration["MongoDbSettings:UsersCollectionName"]
            ?? "Users";

        var database = client.GetDatabase(databaseName);

        _users = database.GetCollection<User>(collectionName);
    }

    // READ: All users
    public async Task<List<User>> GetAllAsync()
    {
        return await _users
            .Find(Builders<User>.Filter.Empty)
            .SortBy(u => u.UserCode)
            .ToListAsync();
    }

    // READ: One user
    public async Task<User?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    // Check duplicate UserCode
    public async Task<bool> UserCodeExistsAsync(
        string userCode, string? excludeId = null)
    {
        var filter = Builders<User>.Filter
            .Eq(u => u.UserCode, userCode);

        if (!string.IsNullOrWhiteSpace(excludeId))
        {
            filter &= Builders<User>.Filter
                .Ne(u => u.Id, excludeId);
        }

        return await _users
            .Find(filter)
            .AnyAsync();
    }

    // CREATE
    public async Task CreateAsync(User user)
    {
        user.Id = null;
        user.UserCode = user.UserCode.Trim();

        await _users.InsertOneAsync(user);
    }

    // UPDATE
    public async Task<bool> UpdateAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Id) ||
            !ObjectId.TryParse(user.Id, out _))
            return false;

        user.UserCode = user.UserCode.Trim();

        var result = await _users.ReplaceOneAsync(
            u => u.Id == user.Id,
            user);

        return result.MatchedCount > 0;
    }

    // DELETE
    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _users.DeleteOneAsync(u => u.Id == id);

        return result.DeletedCount > 0;
    }

    // Get next numeric UserCode (max numeric UserCode + 1)
    public async Task<string> GetNextUserCodeAsync()
    {
        var codes = await _users
            .Find(Builders<User>.Filter.Empty)
            .Project(u => u.UserCode)
            .ToListAsync();

        var max = codes
            .Select(c => {
                if (int.TryParse(c, out var v)) return v;
                return 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return (max + 1).ToString();
    }
}
