
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TotalDev.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("usercode")]
    [Required(ErrorMessage = "User code is required")]
    [StringLength(50)]
    public string UserCode { get; set; } = string.Empty;

    [BsonElement("username")]
    [Required(ErrorMessage = "User name is required")]
    [StringLength(200)]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("mobile")]
    [StringLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [BsonElement("city")]
    [StringLength(100)]
    public string? City { get; set; }

    [BsonElement("address")]
    [StringLength(500)]
    public string? Address { get; set; }

    [BsonElement("password")]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100)]
    // Stored as string but must contain only numeric characters; controller enforces this rule on save
    public string Password { get; set; } = string.Empty;

    public int IsActive { get; set; }
    public int createdBy { get; set; }
    public DateTime createdOn { get; set; } = DateTime.UtcNow;

    public int modifyby { get; set; }
    public DateTime modifyOn { get; set; } = DateTime.UtcNow;
}
