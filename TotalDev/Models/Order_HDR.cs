using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TotalDev.Models
{
    public class Order_HDR
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Ord_No { get; set; }

        public DateTime Ord_Date { get; set; } 

        public string? CustomerId { get; set; }

        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }

        public int? CreatedBy { get; set; } 

        public DateTime CreatedOn { get; set; }

        public int? Modify_By { get; set; }

        public DateTime Modify_On { get; set; }

    }
    public class Order_Details
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Ref_Ord_No { get; set; }

        public DateTime Ref_Ord_Date { get; set; }

        public string? CustomerId { get; set; }

        public string? Item_Id { get; set; }

        public string? Item_Code { get; set; }

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
