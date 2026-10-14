
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TotalDev.Models;
using TotalDev.Services;

namespace TotalDev.Controllers;

public class ProductsController : Controller
{
    private readonly ProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ProductService service,
        ILogger<ProductsController> logger)
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
            var products = await _service.GetAllAsync();
            Product? editProduct = null;

            if (!string.IsNullOrWhiteSpace(editId))
            {
                editProduct = await _service.GetByIdAsync(editId);

                if (editProduct == null)
                    return NotFound();
            }

            ViewBag.EditProduct = editProduct;
            return View(products);
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Loading products failed.");
            return Problem("Products load nahi ho sake.");
        }
    }

    // Insert or update
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Product product)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.FormProduct = product;
            return View("Index", await _service.GetAllAsync());
        }

        try
        {
            var isNew = string.IsNullOrWhiteSpace(product.Id);

            if (await _service.ItemCodeExistsAsync(
                product.ItemCode.Trim(), product.Id))
            {
                ModelState.AddModelError(
                    nameof(product.ItemCode),
                    "Item Code already exists.");

                ViewBag.FormProduct = product;
                return View("Index", await _service.GetAllAsync());
            }

            if (isNew)
                await _service.CreateAsync(product);
            else if (!await _service.UpdateAsync(product))
                return NotFound();

            TempData["Success"] = isNew
                ? "Product added successfully!"
                : "Product updated successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Saving product failed.");
            return Problem("Product save nahi hua.");
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

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (MongoException ex)
        {
            _logger.LogError(ex, "Deleting product failed.");
            return Problem("Product delete nahi hua.");
        }
    }
}
