using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(UserService userService, ILogger<HomeController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Home");
        }
    }
}
