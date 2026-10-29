using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace TotalDev.Models
{
    public class Order_HDR
    {
        [BsonId]
        public int? Ord_No { get; set; }

        public DateTime Ord_Date { get; set; }

        [ForeignKey("Id")]
        public string? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }

        public int? CreatedBy { get; set; } 

        public DateTime CreatedOn { get; set; }

        public int? Modify_By { get; set; }

        public DateTime Modify_On { get; set; }

        public List<Order_Details> Items { get; set; }
            = new List<Order_Details>();

        public List<Customer> Customers { get; set; }
          = new List<Customer>();

    }
    public class Order_Details
    {
        [BsonId]
        public int? V_No { get; set; }

        public DateTime V_Date { get; set; } = DateTime.UtcNow;

        public int? Ref_Ord_No { get; set; }

        public DateTime Ref_Ord_Date { get; set; }

        public string? CustomerId { get; set; }

        [ForeignKey("Item_Id")]
        public int? Item_Id { get; set; }
        public Product? Product { get; set; }


        

        public string? Unit { get; set; }

        public decimal Qty { get; set; }

        public decimal MRP { get; set; }

        public decimal Amount { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? Modify_By { get; set; }

        public DateTime Modify_On { get; set; }
    }

}
