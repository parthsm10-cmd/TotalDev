using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TotalDev.Models;
using TotalDev.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";

            int sessionUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool isAdmin = string.Equals(userRole,"Admin",StringComparison.OrdinalIgnoreCase);

            bool isCustomer = string.Equals(userRole,"Customer",StringComparison.OrdinalIgnoreCase);

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
                if (sessionUserId <= 0)
                {
                    return RedirectToAction("Login", "Customer");
                }

                model.CustomerId = sessionUserId;
            }

            await LoadFormData();

            return View(model);
        }

        // POST: /Order_HDR/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CreateOrderViewModel model)
        {
            try
            {
                // 1. Get login role and session UserId.
                var userRole = HttpContext.Session.GetString("UserRole") ?? "";

                int sessionUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

                bool isAdmin = string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase);

                bool isCustomer = string.Equals(userRole, "Customer", StringComparison.OrdinalIgnoreCase);


                if (!isAdmin && !isCustomer)
                {
                    return Unauthorized();
                }

                // 3. Customer login: force session UserId.
                if (isCustomer)
                {
                    if (sessionUserId <= 0)
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
                    model.CustomerId == 0)
                {
                    ModelState.AddModelError(
                        nameof(model.CustomerId),
                        "Please select a customer.");
                }

                // 5. Validate that the customer exists.
                if (model.CustomerId != 0)
                {
                    var customers =
                        await _customerService.GetAllAsync();

                    bool customerExists = customers.Any(c => c.Id == model.CustomerId);

                    if (!customerExists)
                    {
                        ModelState.AddModelError(
                            nameof(model.CustomerId),
                            "Invalid customer selected.");
                    }
                }

                // 6. Get items with quantity greater than zero.
                var selectedItems = model.Items?.ToList();

                if (selectedItems == null ||
                    selectedItems.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Please enter quantity for at least one item.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadFormData();
                    return View("Index", model);
                }

                // 7. Load products from database.
                var products = await _productService.GetAllAsync();

                var productLookup = products.ToDictionary(p => p.Id.ToString(), p => p);

                decimal totalQty = 0;
                decimal totalAmount = 0;

                var detailList = new List<Order_Details>();

                // 8. Prepare order details.
                foreach (var item in selectedItems!)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemId) ||
                        !productLookup.TryGetValue(item.ItemId, out var product))
                    {
                        ModelState.AddModelError(string.Empty, "One or more selected products are invalid.");
                        continue;
                    }

                    //if (item.Qty <= 0)
                    //    continue;

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
                        Amount = amount,
                        CustomerId = model.CustomerId,
                        CreatedBy = sessionUserId
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
                    CreatedBy = sessionUserId,
                    CreatedOn = DateTime.UtcNow,
                    Modify_On = DateTime.UtcNow
                };

                // 11. Save order header and details.
                await _orderService.CreateOrderAsync(header, detailList);

                TempData["Success"] =
                    "Order created successfully.";

                if (isAdmin)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Customer");
                }
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

        [HttpGet]
        public async Task<IActionResult> Edit(int ordNo,int customerId)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";

            int sessionUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool isAdmin = string.Equals(userRole, "Admin",StringComparison.OrdinalIgnoreCase);

            bool isCustomer = string.Equals(userRole, "Customer",StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isCustomer)
                return Unauthorized();

            // Customer cannot edit another customer's order
            if (isCustomer)
            {
                if (sessionUserId <= 0)
                    return RedirectToAction("Login", "Account");

                customerId = sessionUserId;
            }

            if (customerId <= 0)
                return BadRequest("Customer code is required.");

            // Get order using both Ord_No and CustomerId
            var header = await _orderService.GetOrderForEditAsync(
                ordNo,
                customerId
            );

            if (header == null)
                return NotFound("Order not found.");

            // Get existing order details
            var orderDetails = await _orderService.GetOrderDetailsAsync(ordNo,customerId);

            var products =  await _productService.GetAllAsync();

            // Convert existing order details into CreateOrderViewModel
            var model = new CreateOrderViewModel
            {
                Ord_Date = header.Ord_Date,
                CustomerId = header.CustomerId,

                Items = products.Select(p =>
                {
                    var existingItem = orderDetails.FirstOrDefault(
                        d => d.Item_Id == Convert.ToInt32(p.ItemCode)
                    );

                    return new CreateOrderItemViewModel
                    {
                        ItemId = p.Id.ToString(),
                        Qty = existingItem?.Qty ?? 0
                    };

                }).ToList()
            };

            // Store order number for update
            ViewBag.Ord_No = ordNo;
            ViewBag.IsEdit = true;

            await LoadFormData();

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int ordNo, int customerId, CreateOrderViewModel model)
        {
            try
            {
                var userRole =
                    HttpContext.Session.GetString("UserRole") ?? "";

                var sessionUserId =
                    HttpContext.Session.GetInt32("UserId");

                bool isAdmin = string.Equals(
                    userRole, "Admin",
                    StringComparison.OrdinalIgnoreCase);

                bool isCustomer = string.Equals(
                    userRole, "Customer",
                    StringComparison.OrdinalIgnoreCase);

                if (!isAdmin && !isCustomer)
                    return Unauthorized();

                // Customer can edit only own order
                if (isCustomer)
                {
                    if (!sessionUserId.HasValue)
                        return RedirectToAction("Login", "Account");

                    customerId = sessionUserId.Value;
                }

                // Verify order number and customer code
                var existingHeader =
                    await _orderService.GetOrderForEditAsync(
                        ordNo,
                        customerId
                    );

                if (existingHeader == null)
                    return NotFound("Order not found.");

                // Always use verified customer ID
                model.CustomerId = existingHeader.CustomerId;

                if (isAdmin && model.CustomerId == 0)
                {
                    ModelState.AddModelError(nameof(model.CustomerId),"Please select a customer.");
                }

                var selectedItems = model.Items?.ToList();

                if (selectedItems == null || selectedItems.Count == 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Please enter quantity for at least one item.");
                }

                var products = await _productService.GetAllAsync();

                var productLookup = products.ToDictionary(
                    p => p.Id.ToString(),
                    p => p
                );

                decimal totalQty = 0;
                decimal totalAmount = 0;

                var detailList = new List<Order_Details>();

                foreach (var item in selectedItems ?? new())
                {
                    if (string.IsNullOrWhiteSpace(item.ItemId) || !productLookup.TryGetValue(item.ItemId, out var product))
                    {
                        ModelState.AddModelError(string.Empty,"One or more selected products are invalid.");
                        continue;
                    }

                    //if (item.Qty <= 0)
                    //    continue;

                    // Always use database MRP
                    decimal rate = product.MRP;

                    decimal amount = item.Qty * rate;

                    totalQty += item.Qty;
                    totalAmount += amount;

                    detailList.Add(new Order_Details
                    {
                        Item_Id = Convert.ToInt32(product.ItemCode),
                        CustomerId = existingHeader.CustomerId,
                        Unit = product.Unit,
                        Qty = item.Qty,
                        MRP = rate,
                        Amount = amount,
                        Modify_By = sessionUserId
                    });
                }

                if (!ModelState.IsValid || detailList.Count == 0)
                {
                    await LoadFormData();

                    ViewBag.Ord_No = ordNo;
                    ViewBag.IsEdit = true;

                    return View("Index", model);
                }

                // Prepare updated header
                var header = new Order_HDR
                {
                    Ord_Date = model.Ord_Date,
                    CustomerId = existingHeader.CustomerId,
                    TotalQty = totalQty,
                    TotalAmount = totalAmount,
                    Modify_By = sessionUserId,
                    Modify_On = DateTime.UtcNow
                };

                // Update existing order, do not create a new order
                await _orderService.UpdateOrderAsync(
                    ordNo,
                    existingHeader.CustomerId,
                    header,
                    detailList);

                TempData["Success"] = "Order updated successfully.";
                if (isAdmin)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Customer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error while updating order {OrdNo}", ordNo);
                ModelState.AddModelError(string.Empty,"Order could not be updated. Please try again.");
                await LoadFormData();
                ViewBag.Ord_No = ordNo;
                ViewBag.IsEdit = true;
                return View("Index", model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> OrderShow(DateTime? fromDate,DateTime? toDate)
        {
            var from = fromDate?.Date ?? DateTime.Today;
            var to = toDate?.Date ?? DateTime.Today;

            if (from > to)
            {
                ModelState.AddModelError(
                    "",
                    "From Date cannot be greater than To Date.");

                return View(new OrderShowViewModel
                {
                    FromDate = from,
                    ToDate = to
                });
            }

            var orders = await _orderService.GetOrdersByDateRangeAsync(from,to);

            var model = BuildOrderShowReport(orders, from, to);

            return View(model);
        }


        //Report Actions
        private OrderShowViewModel BuildOrderShowReport(
    List<Order_HDR> orders,
    DateTime from,
    DateTime to)
        {
            var model = new OrderShowViewModel
            {
                FromDate = from,
                ToDate = to
            };

            // Get all unique products from selected orders
            var itemColumns = orders
                .SelectMany(o => o.Items)
                .Where(i => i.Product != null)
                .GroupBy(i => i.Item_Id)
                .Select(g => new OrderShowItemColumn
                {
                    ItemId = g.Key,
                    ItemName = g.First().Product!.ItemName
                })
                .OrderBy(x => x.ItemName)
                .ToList();

            model.ItemColumns = itemColumns;

            foreach (var order in orders)
            {
                var row = new OrderShowRow
                {
                    Ord_No = order.Ord_No,
                    CustomerName = order.Customer?.CustName ?? "",
                    TotalQty = order.TotalQty,
                    TotalAmount = order.TotalAmount
                };

                // Item-wise quantity for each order
                row.ItemQty = order.Items
                    .GroupBy(i => i.Item_Id)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(x => x.Qty)
                    );

                model.Orders.Add(row);
            }

            // Calculate item-wise grand totals
            foreach (var item in model.ItemColumns)
            {
                item.TotalQty = model.Orders.Sum(o =>
                    o.ItemQty.TryGetValue(item.ItemId, out var qty)
                        ? qty
                        : 0
                );
            }

            model.GrandTotalQty =
                model.Orders.Sum(o => o.TotalQty);

            model.GrandTotalAmount =
                model.Orders.Sum(o => o.TotalAmount);

            return model;
        }
        [HttpGet]
        public async Task<IActionResult> DownloadOrderShowPdf(
    DateTime fromDate,
    DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
                return BadRequest("Invalid date range.");

            var orders = await _orderService.GetOrdersByDateRangeAsync(
                fromDate,
                toDate
            );

            var model = BuildOrderShowReport(
                orders,
                fromDate.Date,
                toDate.Date
            );

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .AlignCenter()
                                .Text("DATE WISE CUSTOMER ORDER REPORT")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .AlignCenter()
                                .Text(
                                    $"From: {fromDate:dd/MM/yyyy} " +
                                    $" To: {toDate:dd/MM/yyyy}"
                                )
                                .FontSize(10);

                            column.Item().PaddingVertical(5)
                                .LineHorizontal(1);
                        });

                    page.Content().Column(content =>
                    {
                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(45);
                                columns.RelativeColumn(2);

                                foreach (var item in model.ItemColumns)
                                {
                                    columns.RelativeColumn(1);
                                }

                                columns.ConstantColumn(55);
                                columns.ConstantColumn(70);
                            });

                            // Table header
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle)
                                    .Text("Ord No").Bold();

                                header.Cell().Element(HeaderStyle)
                                    .Text("Customer Name").Bold();

                                foreach (var item in model.ItemColumns)
                                {
                                    header.Cell().Element(HeaderStyle)
                                        .Text(item.ItemName).Bold();
                                }

                                header.Cell().Element(HeaderStyle)
                                    .Text("Total Qty").Bold();

                                header.Cell().Element(HeaderStyle)
                                    .Text("Amount").Bold();
                            });

                            // Order rows
                            foreach (var order in model.Orders)
                            {
                                table.Cell().Element(CellStyle)
                                    .Text(order.Ord_No?.ToString() ?? "");

                                table.Cell().Element(CellStyle)
                                    .Text(order.CustomerName);

                                foreach (var item in model.ItemColumns)
                                {
                                    var qty = order.ItemQty.TryGetValue(
                                        item.ItemId,
                                        out var value) ? value : 0;

                                    table.Cell().Element(CellStyle)
                                        .AlignRight()
                                        .Text(qty.ToString("0.##"));
                                }

                                table.Cell().Element(CellStyle)
                                    .AlignRight()
                                    .Text(order.TotalQty.ToString("0.##"));

                                table.Cell().Element(CellStyle)
                                    .AlignRight()
                                    .Text(order.TotalAmount.ToString("N2"));
                            }

                            // Grand total row
                            table.Cell().Element(TotalStyle)
                                .Text("TOTAL").Bold();

                            table.Cell().Element(TotalStyle)
                                .Text("").Bold();

                            foreach (var item in model.ItemColumns)
                            {
                                table.Cell().Element(TotalStyle)
                                    .AlignRight()
                                    .Text(item.TotalQty.ToString("0.##"))
                                    .Bold();
                            }

                            table.Cell().Element(TotalStyle)
                                .AlignRight()
                                .Text(model.GrandTotalQty.ToString("0.##"))
                                .Bold();

                            table.Cell().Element(TotalStyle)
                                .AlignRight()
                                .Text(model.GrandTotalAmount.ToString("N2"))
                                .Bold();
                        });

                        content.Item().PaddingTop(10)
                            .AlignRight()
                            .Text(
                                $"Grand Total Amount: " +
                                model.GrandTotalAmount.ToString("N2")
                            )
                            .Bold()
                            .FontSize(11);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            byte[] fileBytes = pdf.GeneratePdf();

            string fileName =
                $"Order_Report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";

            return File(
                fileBytes,
                "application/pdf",
                fileName
            );
        }

        // PDF cell styles
        private static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Border(0.5f)
                .BorderColor(Colors.Grey.Medium)
                .Padding(5)
                .AlignMiddle();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .AlignMiddle();
        }

        private static IContainer TotalStyle(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .Border(0.5f)
                .BorderColor(Colors.Grey.Medium)
                .Padding(5)
                .AlignMiddle();
        }
    }
}