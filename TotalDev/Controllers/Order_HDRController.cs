using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers
{
    public class Order_HDRController : Controller
    {
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;
        private readonly Order_HDRService _orderService;
        private readonly ILogger<Order_HDRController> _logger;

        public Order_HDRController(
            CustomerService customerService,
            ProductService productService,
            Order_HDRService orderService,
            ILogger<Order_HDRController> logger)
        {
            _customerService = customerService;
            _productService = productService;
            _orderService = orderService;
            _logger = logger;
        }

        // GET: /Order_HDR/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userRole =
                HttpContext.Session.GetString("UserRole") ?? "";

            var sessionUserId =
                HttpContext.Session.GetString("UserId");

            bool isAdmin = string.Equals(
                userRole,
                "Admin",
                StringComparison.OrdinalIgnoreCase);

            bool isCustomer = string.Equals(
                userRole,
                "Customer",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isCustomer)
            {
                return Unauthorized();
            }

            var model = new CreateOrderViewModel
            {
                Ord_Date = DateTime.Today
            };

            // Customer login: automatically select own customer.
            if (isCustomer)
            {
                if (string.IsNullOrWhiteSpace(sessionUserId))
                {
                    return RedirectToAction("Login", "Account");
                }

                model.CustomerId = sessionUserId;
            }

            await LoadFormData();

            return View(model);
        }

        // POST: /Order_HDR/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            CreateOrderViewModel model)
        {
            try
            {
                // 1. Get login role and session UserId.
                var userRole =
                    HttpContext.Session.GetString("UserRole") ?? "";

                var sessionUserId =
                    HttpContext.Session.GetString("UserId");

                bool isAdmin = string.Equals(
                    userRole,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);

                bool isCustomer = string.Equals(
                    userRole,
                    "Customer",
                    StringComparison.OrdinalIgnoreCase);

                // 2. Allow only Admin and Customer.
                if (!isAdmin && !isCustomer)
                {
                    return Unauthorized();
                }

                // 3. Customer login: force session UserId.
                if (isCustomer)
                {
                    if (string.IsNullOrWhiteSpace(sessionUserId))
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Session expired. Please login again.");
                    }
                    else
                    {
                        // Ignore posted CustomerId.
                        model.CustomerId = sessionUserId;
                    }
                }

                // 4. Admin must select a customer.
                if (isAdmin &&
                    string.IsNullOrWhiteSpace(model.CustomerId))
                {
                    ModelState.AddModelError(
                        nameof(model.CustomerId),
                        "Please select a customer.");
                }

                // 5. Validate that the customer exists.
                if (!string.IsNullOrWhiteSpace(model.CustomerId))
                {
                    var customers =
                        await _customerService.GetAllAsync();

                    bool customerExists = customers.Any(c =>
                        c.Id.ToString() == model.CustomerId);

                    if (!customerExists)
                    {
                        ModelState.AddModelError(
                            nameof(model.CustomerId),
                            "Invalid customer selected.");
                    }
                }

                // 6. Get items with quantity greater than zero.
                var selectedItems = model.Items?
                    .Where(x => x.Qty > 0)
                    .ToList();

                if (selectedItems == null ||
                    selectedItems.Count == 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Please enter quantity for at least one item.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadFormData();
                    return View("Index", model);
                }

                // 7. Load products from database.
                var products =
                    await _productService.GetAllAsync();

                var productLookup = products.ToDictionary(
                    p => p.Id.ToString(),
                    p => p);

                decimal totalQty = 0;
                decimal totalAmount = 0;

                var detailList = new List<Order_Details>();

                // 8. Prepare order details.
                foreach (var item in selectedItems!)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemId) ||
                        !productLookup.TryGetValue(
                            item.ItemId,
                            out var product))
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "One or more selected products are invalid.");

                        continue;
                    }

                    if (item.Qty <= 0)
                        continue;

                    // Always use database MRP.
                    decimal rate = product.MRP;

                    decimal amount = item.Qty * rate;

                    totalQty += item.Qty;
                    totalAmount += amount;

                    detailList.Add(new Order_Details
                    {
                        Item_Id = Convert.ToInt32(product.ItemCode),
                        Unit = product.Unit,
                        Qty = item.Qty,
                        MRP = rate,
                        Amount = amount
                    });
                }

                // 9. Stop saving if product validation fails.
                if (!ModelState.IsValid ||
                    detailList.Count == 0)
                {
                    await LoadFormData();
                    return View("Index", model);
                }

                // 10. Prepare order header.
                var header = new Order_HDR
                {
                    Ord_Date = model.Ord_Date,

                    // Customer ID comes from selected Admin value
                    // or forced session UserId for Customer.
                    CustomerId = model.CustomerId,

                    TotalQty = totalQty,
                    TotalAmount = totalAmount,
                    CreatedOn = DateTime.UtcNow,
                    Modify_On = DateTime.UtcNow
                };

                // 11. Save order header and details.
                await _orderService.CreateOrderAsync(
                    header,
                    detailList);

                TempData["Success"] =
                    "Order created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while creating an order.");

                ModelState.AddModelError(
                    string.Empty,
                    "Order could not be created. Please try again.");

                await LoadFormData();

                return View("Index", model);
            }
        }

        // 12. Load dropdown and product data.
        private async Task LoadFormData()
        {
            ViewBag.Customers =
                await _customerService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();
        }
    }
}