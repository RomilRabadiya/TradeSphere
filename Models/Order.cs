using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSphere3.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }  // Primary Key

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Default to now

        // FK to Product
        public int? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;  // Default = 1

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }  // Unit Price at order time

        [NotMapped] // Computed, not stored
        public decimal TotalAmount => Quantity * Price;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Order Status

        [Required]
        [MaxLength(300)]
        public string ShippingAddress { get; set; }  // Delivery address

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Cash on Delivery"; // Payment type

        // FK to ApplicationUser
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }



        // NEW: Keep track of Trader even if Product is deleted
        public int? TraderId { get; set; }

        [ForeignKey("TraderId")]
        public virtual Trader Trader { get; set; }
    }
}