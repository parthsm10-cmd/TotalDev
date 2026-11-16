using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers
{
    public class HomeController : Controller
    {
        private readonly Order_HDRService _orderService;
        private readonly UserService _userService;
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            UserService userService,
            Order_HDRService orderService,
            CustomerService customerService,
            ProductService productService,
            ILogger<HomeController> logger)
        {
            _userService = userService;
            _orderService = orderService;
            _customerService = customerService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? date)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("CustomerHome", "Customer");

                return RedirectToAction(nameof(Login));
            }

            var reportDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (!DateTime.TryParseExact(
                    date.Trim(),
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out reportDate))
                {
                    return BadRequest("Date must use the dd-MM-yyyy format.");
                }

                reportDate = reportDate.Date;
            }

            try
            {
                var orders = await _orderService.GetOrdersByDateRangeAsync(reportDate, reportDate);
                var model = new AdminDashboardViewModel
                {
                    ReportDate = reportDate,
                    CustomerCount = await _customerService.GetCountAsync(),
                    ProductCount = await _productService.GetCountAsync(),
                    UserCount = await _userService.GetCountAsync(),
                    OrderCount = orders.Count,
                    TotalQuantity = orders.Sum(order => order.TotalQty),
                    TotalSales = orders.Sum(order => order.TotalAmount),
                    Orders = orders
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading the admin dashboard failed.");
                return Problem("Dashboard load failed.");
            }
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
                HttpContext.Session.SetInt32("UserId", 999);
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
                    HttpContext.Session.SetInt32("UserId", user.UserCode);
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
