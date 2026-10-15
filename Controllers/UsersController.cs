using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers;

public class UsersController : Controller
{
    private readonly IMongoClient _client;
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMongoClient client, IConfiguration configuration, ILogger<UsersController> logger)
    {
        _client = client;
        _logger = logger;

        var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? throw new InvalidOperationException("Database name missing.");
        var collectionName = configuration["MongoDbSettings:UsersCollectionName"] ?? "Users";

        var database = _client.GetDatabase(databaseName);
        _users = database.GetCollection<User>(collectionName);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? editId = null)
    {
        try
        {
            var users = await _users.Find(Builders<User>.Filter.Empty).SortBy(u => u.UserName).ToListAsync();
            User? editUser = null;

            if (!string.IsNullOrWhiteSpace(editId))
            {
                if (!ObjectId.TryParse(editId, out _))
                    return NotFound();

                editUser = await _users.Find(u => u.Id == editId).FirstOrDefaultAsync();
                if (editUser == null)
                    return NotFound();
            }

            ViewBag.EditUser = editUser;
            return View(users);
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Loading users failed.");
            return Problem("Users load failed.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(User user, string isActiveToggle)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.FormUser = user;
            return View("Index", await _users.Find(Builders<User>.Filter.Empty).ToListAsync());
        }

        try
        {
            // convert toggle to int
            user.IsActive = (isActiveToggle == "on") ? 1 : 0;

            var isNew = string.IsNullOrWhiteSpace(user.Id);

            if (isNew)
            {
                user.Id = null; // ensure new
                await _users.InsertOneAsync(user);
                TempData["Success"] = "User added successfully!";
            }
            else
            {
                if (!ObjectId.TryParse(user.Id, out _))
                    return NotFound();

                var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
                if (result.MatchedCount == 0)
                    return NotFound();

                TempData["Success"] = "User updated successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Saving user failed.");
            return Problem("User save failed.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            if (!ObjectId.TryParse(id, out _))
                return NotFound();

            var result = await _users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0)
                return NotFound();

            TempData["Success"] = "User deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Deleting user failed.");
            return Problem("User delete failed.");
        }
    }
}
