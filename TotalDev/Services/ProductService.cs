
using MongoDB.Bson;
using MongoDB.Driver;
using TotalDev.Models;

namespace TotalDev.Services;

public class ProductService
{
    private readonly IMongoCollection<Product> _products;

    public ProductService(
        IMongoClient client,
        IConfiguration configuration)
    {
        var databaseName =
            configuration["MongoDbSettings:DatabaseName"]
            ?? throw new InvalidOperationException(
                "Database name missing.");

        var collectionName =
            configuration["MongoDbSettings:ProductsCollectionName"]
            ?? throw new InvalidOperationException(
                "Collection name missing.");

        var database = client.GetDatabase(databaseName);

        _products = database.GetCollection<Product>(
            collectionName);
    }

    // READ: All products
    public async Task<List<Product>> GetAllAsync()
    {
        return await _products
            .Find(Builders<Product>.Filter.Empty)
            .SortBy(p => p.ItemName)
            .ToListAsync();
    }

    // READ: One product
    public async Task<Product?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _products
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync();
    }

    // Check duplicate ItemCode
    public async Task<bool> ItemCodeExistsAsync(
        string itemCode, string? excludeId = null)
    {
        var filter = Builders<Product>.Filter
            .Eq(p => p.ItemCode, itemCode);

        if (!string.IsNullOrWhiteSpace(excludeId))
        {
            filter &= Builders<Product>.Filter
                .Ne(p => p.Id, excludeId);
        }

        return await _products
            .Find(filter)
            .AnyAsync();
    }

    // CREATE
    public async Task CreateAsync(Product product)
    {
        product.Id = null;
        product.ItemCode = product.ItemCode.Trim();

        await _products.InsertOneAsync(product);
    }

    // UPDATE
    public async Task<bool> UpdateAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Id) ||
            !ObjectId.TryParse(product.Id, out _))
            return false;

        product.ItemCode = product.ItemCode.Trim();

        var result = await _products.ReplaceOneAsync(
            p => p.Id == product.Id,
            product);

        return result.MatchedCount > 0;
    }

    // DELETE
    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _products.DeleteOneAsync(
            p => p.Id == id);

        return result.DeletedCount > 0;
    }

    // Get next numeric ItemCode (max numeric ItemCode + 1)
    public async Task<string> GetNextItemCodeAsync()
    {
        // Project only ItemCode values
        var codes = await _products
            .Find(Builders<Product>.Filter.Empty)
            .Project(p => p.ItemCode)
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
