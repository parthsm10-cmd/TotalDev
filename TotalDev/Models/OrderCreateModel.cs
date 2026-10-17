using System.ComponentModel.DataAnnotations;

namespace TotalDev.Models
{
    public class OrderItemModel
    {
        public string? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? Unit { get; set; }
        public decimal Qty { get; set; }
        public decimal MRP { get; set; }
    }

    public class OrderCreateModel
    {
        [Required]
        public string? CustomerId { get; set; }

        public DateTime Ord_Date { get; set; } = DateTime.UtcNow;

        public List<OrderItemModel> Items { get; set; } = new List<OrderItemModel>();
    }
}
