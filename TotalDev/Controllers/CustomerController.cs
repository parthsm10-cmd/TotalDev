using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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
            catch (MongoException ex)
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
            catch (MongoException ex)
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
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Deleting customer failed.");
                return Problem("Customer delete failed.");
            }
        }
    }
}
