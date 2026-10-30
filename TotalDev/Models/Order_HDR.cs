namespace TotalDev.Models
{
    public class Order_HDR
    {
        public int Ord_No { get; set; }

        public DateTime Ord_Date { get; set; }

        public string CustomerId { get; set; } = string.Empty;

        public Customer? Customer { get; set; }

        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }

        public int? CreatedBy { get; set; } 

        public DateTime CreatedOn { get; set; }

        public int? Modify_By { get; set; }

        public DateTime Modify_On { get; set; }

        public List<Order_Details> Items { get; set; }
            = new List<Order_Details>();
    }

    public class Order_Details
    {
        public int V_No { get; set; }

        public DateTime V_Date { get; set; } = DateTime.UtcNow;

        public int Ref_Ord_No { get; set; }

        public DateTime Ref_Ord_Date { get; set; }

        public string? CustomerId { get; set; }

        public int Item_Id { get; set; }

        public Product? Product { get; set; }

        public Order_HDR? Order { get; set; }
        
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
