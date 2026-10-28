
using System;
using System.Collections.Generic;

namespace TotalDev.Models
{
    public class CreateOrderViewModel
    {
        public DateTime Ord_Date { get; set; } = DateTime.Today;

        public string? CustomerId { get; set; }

        public List<CreateOrderItemViewModel> Items { get; set; }
            = new List<CreateOrderItemViewModel>();
    }

    public class CreateOrderItemViewModel
    {
        public string? ItemId { get; set; }

        public string? ItemCode { get; set; }

        public string? Unit { get; set; }

        public decimal Qty { get; set; }

        public decimal MRP { get; set; }
    }
}
