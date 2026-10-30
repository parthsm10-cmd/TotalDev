
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
            await LoadFormData();

            return View(new CreateOrderViewModel
            {
                Ord_Date = DateTime.Today
            });
        }

        // POST: /Order_HDR/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CreateOrderViewModel model)
        {
            try
            {
                // 1. Get customer ID from session if required.
                var sessionCustomerId =
                    HttpContext.Session.GetString("UserId");

                if (string.IsNullOrWhiteSpace(model.CustomerId) &&
                    !string.IsNullOrWhiteSpace(sessionCustomerId))
                {
                    model.CustomerId = sessionCustomerId;
                }

                // 2. Validate customer.
                if (string.IsNullOrWhiteSpace(model.CustomerId))
                {
                    ModelState.AddModelError(
                        nameof(model.CustomerId),
                        "Please select a customer.");
                }

                // 3. Get only items with quantity greater than zero.
                var selectedItems = model.Items?
                    .Where(x => x.Qty > 0)
                    .ToList();

                if (selectedItems == null || selectedItems.Count == 0)
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

                // 4. Load products from SQL Server.
                var products = await _productService.GetAllAsync();

                var productLookup = products.ToDictionary(
                    p => p.Id.ToString(),
                    p => p);

                decimal totalQty = 0;
                decimal totalAmount = 0;

                var detailList = new List<Order_Details>();

                // 5. Prepare item-wise order details.
                foreach (var item in selectedItems!)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemId) ||
                        !productLookup.TryGetValue(
                            item.ItemId, out var product))
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "One or more selected products are invalid.");

                        continue;
                    }

                    if (item.Qty <= 0)
                        continue;

                    // Use database price rather than posted price.
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

                // Do not save if any submitted product is invalid.
                if (!ModelState.IsValid || detailList.Count == 0)
                {
                    await LoadFormData();
                    return View("Index", model);
                }

                // 6. Prepare main order header.
                var header = new Order_HDR
                {
                    Ord_Date = model.Ord_Date,
                    CustomerId = model.CustomerId,
                    TotalQty = totalQty,
                    TotalAmount = totalAmount,
                    CreatedOn = DateTime.UtcNow,
                    Modify_On = DateTime.UtcNow

                    // Set CreatedBy here if you store the
                    // logged-in user's numeric ID in session.
                };

                // 7. Save header and details using OrderService.
                await _orderService.CreateOrderAsync(
                    header, detailList);

                TempData["Success"] =
                    "Order created successfully.";

                // 8. Redirect to the existing Index action.
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "An error occurred while creating an order.");

                ModelState.AddModelError(
                    string.Empty,
                    "Order could not be created. Please try again.");

                await LoadFormData();
                return View("Index", model);
            }
        }

        // Reload dropdown and product data for the view.
        private async Task LoadFormData()
        {
            ViewBag.Customers =
                await _customerService.GetAllAsync();

            ViewBag.Products =
                await _productService.GetAllAsync();
        }
    }
}
