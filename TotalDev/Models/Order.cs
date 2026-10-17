using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
namespace TotalDev.Models
{
    public class OrderItem
    {
        public string? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? Unit { get; set; }
        public decimal Qty { get; set; }
        public decimal MRP { get; set; }
        public decimal Amount { get; set; }
    }

    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? CustomerId { get; set; }

        public DateTime Ord_Date { get; set; } = DateTime.UtcNow;

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime createdOn { get; set; } = DateTime.UtcNow;

        public string customerName { get; set; } = string.Empty;
    }
}
