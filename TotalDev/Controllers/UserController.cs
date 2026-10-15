using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _service;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserService service,
            ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // List and form
        [HttpGet]
        public async Task<IActionResult> Index(string? editId = null)
        {
            try
            {
                var users = await _service.GetAllAsync();
                User? editUser = null;

                if (!string.IsNullOrWhiteSpace(editId))
                {
                    editUser = await _service.GetByIdAsync(editId);

                    if (editUser == null)
                        return NotFound();
                }

                // If not editing, prefill UserCode with next numeric code
                if (string.IsNullOrWhiteSpace(editId) && editUser == null)
                {
                    var nextCode = await _service.GetNextUserCodeAsync();
                    editUser = new User { UserCode = nextCode };
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

        // Insert or update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FormUser = user;
                return View("Index", await _service.GetAllAsync());
            }

            // Enforce numeric-only password (stored as string)
            if (string.IsNullOrWhiteSpace(user.Password) || !user.Password.All(char.IsDigit))
            {
                ModelState.AddModelError(nameof(user.Password), "Password must contain only numeric characters.");
                ViewBag.FormUser = user;
                return View("Index", await _service.GetAllAsync());
            }

            try
            {
                var isNew = string.IsNullOrWhiteSpace(user.Id);

                if (await _service.UserCodeExistsAsync(
                    user.UserCode.Trim(), user.Id))
                {
                    ModelState.AddModelError(
                        nameof(user.UserCode),
                        "User Code already exists.");

                    ViewBag.FormUser = user;
                    return View("Index", await _service.GetAllAsync());
                }

                if (isNew)
                    await _service.CreateAsync(user);
                else if (!await _service.UpdateAsync(user))
                    return NotFound();

                TempData["Success"] = isNew
                    ? "User added successfully!"
                    : "User updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Saving user failed.");
                return Problem("User save failed.");
            }
        }

        // Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (!await _service.DeleteAsync(id))
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
}
