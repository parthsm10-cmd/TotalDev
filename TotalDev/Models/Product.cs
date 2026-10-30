using System.ComponentModel.DataAnnotations;

namespace TotalDev.Models;

public class Product
{
    [Key]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Item code is required")]
    public int ItemCode { get; set; }

    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100)]
    public string ItemName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter unit")]
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Qty { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal MRP { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    public int IsActive { get; set; }
    public int createdBy { get; set; }
    public DateTime createdOn { get; set; } = DateTime.UtcNow;
    public int modifyby { get; set; }
    public DateTime modifyOn { get; set; } = DateTime.UtcNow;

    public ICollection<Order_Details> OrderDetails { get; set; } = new List<Order_Details>();
}
