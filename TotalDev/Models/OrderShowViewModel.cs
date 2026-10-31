
namespace TotalDev.Models
{
    public class OrderShowViewModel
    {
        public DateTime FromDate { get; set; } = DateTime.Today;

        public DateTime ToDate { get; set; } = DateTime.Today;

        public List<OrderShowItemColumn> ItemColumns { get; set; }
            = new();

        public List<OrderShowRow> Orders { get; set; }
            = new();

        public decimal GrandTotalQty { get; set; }

        public decimal GrandTotalAmount { get; set; }
    }

    public class OrderShowItemColumn
    {
        public int ItemId { get; set; }

        public string ItemName { get; set; } = "";

        public decimal TotalQty { get; set; }
    }

    public class OrderShowRow
    {
        public int? Ord_No { get; set; }

        public string CustomerName { get; set; } = "";

        public Dictionary<int, decimal> ItemQty { get; set; }
            = new();

        public decimal TotalQty { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
