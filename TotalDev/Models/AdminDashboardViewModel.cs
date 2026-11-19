namespace TotalDev.Models;

public sealed class AdminDashboardViewModel
{
    public DateTime ReportDate { get; set; }
    public int CustomerCount { get; set; }
    public int ProductCount { get; set; }
    public int UserCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalSales { get; set; }
    public int PendingOrder { get; set; }
    public List<Customer> PendingCustomers { get; set; } = new();
    public List<Order_HDR> Orders { get; set; } = [];
}
