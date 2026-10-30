using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers
{
    public class HomeController : Controller
    {
        private readonly Order_HDRService _orderService;
        private readonly UserService _userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(UserService userService, Order_HDRService orderService, ILogger<HomeController> logger)
        {
            _userService = userService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Show recent orders directly on admin home page
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var orders = await _orderService.GetAllAsync();
                return View(orders);
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllAsync();

            // For display, we may want to resolve customer name; keep CustomerId for now
            return View("OrdersList", orders);
        }

        public IActionResult Privacy()
        {
            return View();
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
                HttpContext.Session.SetString("UserRole", "Admin");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var users = await _userService.GetAllAsync();
                var user = users.FirstOrDefault(u => string.Equals(u.UserName, name.Trim(), StringComparison.OrdinalIgnoreCase)
                                                    && u.Password == password);

                if (user != null)
                {
                    // store user id in session
                    HttpContext.Session.SetString("UserId", user.Id ?? string.Empty);
                    // store username for display in layout
                    HttpContext.Session.SetString("UserName", user.UserName ?? string.Empty);
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction(nameof(Index));
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Logout()
        {
            
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Home");
            }
            else
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Customer");
            }
        }
    }
}
