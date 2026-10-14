
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TotalDev.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Item code is required")]
    [StringLength(30)]
    public string ItemCode { get; set; } = string.Empty;

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
}
