using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerService _service;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            CustomerService service,
            ILogger<CustomerController> logger)
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
                var customers = await _service.GetAllAsync();
                Customer? editCustomer = null;

                if (!string.IsNullOrWhiteSpace(editId))
                {
                    editCustomer = await _service.GetByIdAsync(editId);

                    if (editCustomer == null)
                        return NotFound();
                }

                ViewBag.EditCustomer = editCustomer;
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading customers failed.");
                return Problem("Customers load failed.");
            }
        }

        // Insert or update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FormCustomer = customer;
                return View("Index", await _service.GetAllAsync());
            }

            // Enforce numeric-only password (stored as string)
            if (string.IsNullOrWhiteSpace(customer.Password) || !customer.Password.All(char.IsDigit))
            {
                ModelState.AddModelError(nameof(customer.Password), "Password must contain only numeric characters.");
                ViewBag.FormCustomer = customer;
                return View("Index", await _service.GetAllAsync());
            }

            try
            {
                var isNew = string.IsNullOrWhiteSpace(customer.Id);

                if (isNew)
                    await _service.CreateAsync(customer);
                else if (!await _service.UpdateAsync(customer))
                    return NotFound();

                TempData["Success"] = isNew ? "Customer added successfully!" : "Customer updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving customer failed.");
                return Problem("Customer save failed.");
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

                TempData["Success"] = "Customer deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deleting customer failed.");
                return Problem("Customer delete failed.");
            }
        }

        // GET: /Home/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string name, string password)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Name and password are required.");
                return View();
            }

            // Default credential
            if (string.Equals(name.Trim(), "Dev", StringComparison.OrdinalIgnoreCase) && password == "2026")
            {
                // set session user id to a special value
                HttpContext.Session.SetString("UserId", "default-dev");
                // also store a display name for layout
                HttpContext.Session.SetString("UserName", "Dev");
                HttpContext.Session.SetString("UserRole", "Admin"); // Optional: Set a role for the default user
                return RedirectToAction(nameof(CreateOrder));
            }

            try
            {
                var users = await _service.GetAllAsync();
                var user = users.FirstOrDefault(u => string.Equals(u.CustName, name.Trim(), StringComparison.OrdinalIgnoreCase)
                                                    && u.Password == password);

                if (user != null)
                {
                    // store user id in session
                    HttpContext.Session.SetString("UserId", user.Id ?? string.Empty);
                    // store username for display in layout
                    HttpContext.Session.SetString("UserName", user.CustName ?? string.Empty);
                    HttpContext.Session.SetString("UserRole", "Customer"); // Optional: Set a role for the user
                    return RedirectToAction(nameof(CreateOrder));
                }
                else
                {
                    // No matching user found
                    ModelState.AddModelError(string.Empty, "No user found with provided credentials.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login check failed.");
                return Problem("Login failed due to server error.");
            }
        }
        [HttpGet]
        public IActionResult CustomerHome()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrWhiteSpace(userId) ||
                !string.Equals(userRole, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Login));
            }

            return View();
        }
        [HttpGet]
        public IActionResult CreateOrder()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrWhiteSpace(userId) ||
                !string.Equals(userRole, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Login));
            }

            return RedirectToAction("Index", "Order_HDR");
        }

    }
}
