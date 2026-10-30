using System.ComponentModel.DataAnnotations;

namespace TotalDev.Models;

public class User
{
  [Key]
  public string? Id { get; set; }

  [Required(ErrorMessage = "User code is required")]
  [StringLength(50)]
  public string UserCode { get; set; } = string.Empty;

  [Required(ErrorMessage = "User name is required")]
  [StringLength(200)]
  public string UserName { get; set; } = string.Empty;

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
}
