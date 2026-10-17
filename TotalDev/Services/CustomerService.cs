using MongoDB.Bson;
using MongoDB.Driver;
using TotalDev.Models;

namespace TotalDev.Services;

public class CustomerService
{
    private readonly IMongoCollection<Customer> _customers;

    public CustomerService(
        IMongoClient client,
        IConfiguration configuration)
    {
        var databaseName =
            configuration["MongoDbSettings:DatabaseName"]
            ?? throw new InvalidOperationException("Database name missing.");

        var collectionName =
            configuration["MongoDbSettings:CustomersCollectionName"]
            ?? "Customers";

        var database = client.GetDatabase(databaseName);

        _customers = database.GetCollection<Customer>(collectionName);
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _customers
            .Find(Builders<Customer>.Filter.Empty)
            .SortBy(c => c.CustName)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _customers
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Customer customer)
    {
        customer.Id = null;
        await _customers.InsertOneAsync(customer);
    }

    public async Task<bool> UpdateAsync(Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Id) ||
            !ObjectId.TryParse(customer.Id, out _))
            return false;

        var result = await _customers.ReplaceOneAsync(
            c => c.Id == customer.Id,
            customer);

        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _customers.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}
