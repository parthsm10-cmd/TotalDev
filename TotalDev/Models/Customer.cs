using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TotalDev.Models
{
    public class Customer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("custname")]
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100)]
        public string CustName { get; set; } = string.Empty;

        [BsonElement("mobile")]
        [StringLength(20)]
        public string Mobile { get; set; } = string.Empty;

        [BsonElement("city")]
        [StringLength(100)]
        public string? City { get; set; }

        [BsonElement("address")]
        [StringLength(500)]
        public string? Address { get; set; }

        // 1 = active, 0 = inactive
        public int IsActive { get; set; } = 1;

        [BsonElement("password")]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100)]
        // Stored as string but must contain only numeric characters; controller enforces this rule on save
        public string Password { get; set; } = string.Empty;
    }
}
