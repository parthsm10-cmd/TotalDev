using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Index(int editId)
    {
        try
        {
            var products = await _service.GetAllAsync();
            Product? editProduct = null;

            if (editId > 0)
            {
                editProduct = await _service.GetByIdAsync(editId);

                if (editProduct == null)
                    return NotFound();
            }

            // If not editing, prefill ItemCode with next numeric code
            if (editId > 0 &&editProduct == null)
            {
                var nextCode = await _service.GetNextItemCodeAsync();
                editProduct = new Product { ItemCode = Convert.ToInt32(nextCode) };
            }

            ViewBag.EditProduct = editProduct;
            return View(products);
        }
        catch (Exception ex)
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
        // Normalize IsActive from checkbox (checkbox posts value when checked)
        // Treat any truthy checkbox value as 1, otherwise 0
        var form = HttpContext.Request.Form;
        if (form != null && form.TryGetValue("IsActive", out var isActiveVal))
        {
            // StringValues may contain multiple entries (hidden + checkbox). Take the last value.
            var s = isActiveVal.Count > 0 ? isActiveVal[isActiveVal.Count - 1] : isActiveVal.ToString();
            s = s?.Trim().ToLowerInvariant();
            product.IsActive = (s == "on" || s == "true" || s == "1") ? 1 : 0;
        }

        //if (!ModelState.IsValid)
        //{
        //    ViewBag.FormProduct = product;
        //    return View("Index", await _service.GetAllAsync());
        //}

        try
        {
            var isNew = string.IsNullOrWhiteSpace(product.Id);

            if (await _service.ItemCodeExistsAsync(
                Convert.ToInt32(product.ItemCode), product.Id))
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
        catch (Exception ex)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deleting product failed.");
            return Problem("Product delete nahi hua.");
        }
    }
}
