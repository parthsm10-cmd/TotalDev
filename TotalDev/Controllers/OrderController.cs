using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers;

public class OrderController : Controller
{
    private readonly CustomerService _customerService;
    private readonly ProductService _productService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        CustomerService customerService,
        ProductService productService,
        ILogger<OrderController> logger)
    {
        _customerService = customerService;
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Customers for combobox
        var customers = await _customerService.GetAllAsync();
        ViewBag.Customers = customers;

        // All products for display in table
        var products = await _productService.GetAllAsync();
        ViewBag.Products = products;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Products = await _productService.GetAllAsync();
            return View(model);
        }

        try
        {
            // If customer is logged in (customer placing order), prefer session user id
            var sessionCustomerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(model.CustomerId) && !string.IsNullOrWhiteSpace(sessionCustomerId))
            {
                model.CustomerId = sessionCustomerId;
            }

            // Only consider items with Qty > 0 as selected
            if (model.Items == null)
            {
                ModelState.AddModelError(string.Empty, "Please add at least one item to the order.");
                ViewBag.Customers = await _customerService.GetAllAsync();
                ViewBag.Products = await _productService.GetAllAsync();
                return View(model);
            }

            // Compute totals and collect selected items
            decimal totalQty = 0;
            decimal totalAmount = 0;
            var hasSelected = false;
            foreach (var it in model.Items)
            {
                if (it.Qty > 0)
                {
                    hasSelected = true;
                    totalQty += it.Qty;
                    totalAmount += it.Qty * it.MRP;
                }
            }

            if (!hasSelected)
            {
                ModelState.AddModelError(string.Empty, "Please enter quantity for at least one item.");
                ViewBag.Customers = await _customerService.GetAllAsync();
                ViewBag.Products = await _productService.GetAllAsync();
                return View(model);
            }

            // Collect selected items (Qty > 0)
            var selected = model.Items.Where(i => i.Qty > 0).ToList();

            // persist order as single document with embedded items
            var orderDoc = new Order
            {
                CustomerId = model.CustomerId,
                Ord_Date = model.Ord_Date,
                Items = selected.Select(i => new OrderItem
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    Unit = i.Unit,
                    Qty = i.Qty,
                    MRP = i.MRP,
                    Amount = i.Qty * i.MRP
                }).ToList(),
                TotalQty = totalQty,
                TotalAmount = totalAmount
            };

            var orderService = HttpContext.RequestServices.GetService(typeof(OrderService)) as OrderService;
            if (orderService != null)
            {
                await orderService.CreateAsync(orderDoc);
            }

            TempData["Success"] = "Order created successfully.";
            return RedirectToAction("Create");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Creating order failed.");
            return Problem("Order creation failed.");
        }
    }
}
