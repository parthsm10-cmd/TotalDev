
using MongoDB.Bson;
using MongoDB.Driver;
using TotalDev.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Register MVC
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
});

// 2. Register MongoDB Client
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration =
        sp.GetRequiredService<IConfiguration>();

    var connectionString =
        configuration.GetConnectionString("MongoDb");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "MongoDB connection string is missing in appsettings.json.");
    }

    return new MongoClient(connectionString);
});

// 3. Register Services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CustomerService>();

var app = builder.Build();

// Use session
app.UseSession();

// 4. Test MongoDB connection and create collection if missing
try
{
    var client = app.Services
        .GetRequiredService<IMongoClient>();

    var configuration = app.Configuration;

    var databaseName =
        configuration["MongoDbSettings:DatabaseName"]
        ?? "ShopDB";

    var collectionName =
        configuration["MongoDbSettings:ProductsCollectionName"]
        ?? "Products";

    var database = client.GetDatabase(databaseName);

    // Verify that MongoDB Atlas is reachable
    await database.RunCommandAsync<BsonDocument>(
        new BsonDocument("ping", 1));

    Console.WriteLine(
        "SUCCESS: MongoDB Atlas connected!");

    // Check whether Products collection exists
    var collectionNames = await
        (await database.ListCollectionNamesAsync())
        .ToListAsync();

    if (!collectionNames.Contains(collectionName))
    {
        await database.CreateCollectionAsync(collectionName);

        Console.WriteLine(
            $"SUCCESS: Collection '{collectionName}' created!");
    }
    else
    {
        Console.WriteLine(
            $"INFO: Collection '{collectionName}' already exists.");
    }

    // Ensure Users collection exists
    var usersCollectionName =
        configuration["MongoDbSettings:UsersCollectionName"]
        ?? "Users";

    if (!collectionNames.Contains(usersCollectionName))
    {
        await database.CreateCollectionAsync(usersCollectionName);

        Console.WriteLine(
            $"SUCCESS: Collection '{usersCollectionName}' created!");
    }
    else
    {
        Console.WriteLine(
            $"INFO: Collection '{usersCollectionName}' already exists.");
    }
}
catch (Exception ex)
{
    Console.WriteLine(
        "ERROR: MongoDB connection or collection setup failed.");

    Console.WriteLine(ex.ToString());
}

// 5. Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 6. Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

// 7. Start application
app.Run();
