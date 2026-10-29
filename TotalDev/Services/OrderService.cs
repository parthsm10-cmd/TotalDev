using MongoDB.Bson;
using MongoDB.Driver;
using TotalDev.Models;

namespace TotalDev.Services;

public class OrderService
{
    private readonly IMongoCollection<Order> _orders;

    public OrderService(IMongoClient client, IConfiguration configuration)
    {
        var databaseName = configuration["MongoDbSettings:DatabaseName"]
            ?? throw new InvalidOperationException("Database name missing.");

        var collectionName = configuration["MongoDbSettings:OrdersCollectionName"] ?? "Orders";

        var database = client.GetDatabase(databaseName);
        _orders = database.GetCollection<Order>(collectionName);
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _orders.Find(Builders<Order>.Filter.Empty).SortByDescending(o => o.createdOn).ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Order order)
    {
        order.Id = null;
        order.createdOn = DateTime.UtcNow;
        await _orders.InsertOneAsync(order);
    }
}
