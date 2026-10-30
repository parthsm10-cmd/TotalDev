using System.ComponentModel.DataAnnotations;

namespace TotalDev.Models;

public class Customer
{
    [Key]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(100)]
    public string CustName { get; set; } = string.Empty;

    [StringLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    public int IsActive { get; set; }
    public int createdBy { get; set; }
    public DateTime createdOn { get; set; } = DateTime.UtcNow;
    public int modifyby { get; set; }
    public DateTime modifyOn { get; set; } = DateTime.UtcNow;

    public ICollection<Order_HDR> Orders { get; set; } = new List<Order_HDR>();
}
