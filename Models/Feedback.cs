using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSphere3.Models
{
    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeedbackId { get; set; }  // Primary Key

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty; // User who left the feedback

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }  // Product being reviewed

        [ForeignKey("Order")]
        public int? OrderId { get; set; }  // Optional: linked order

        // ⭐ Feedback fields
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }  // Rating (1–5)

        [StringLength(1000)]
        public string? Comment { get; set; }  // Text feedback

        [DataType(DataType.Date)]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        // 🔗 Navigation (optional — no collection needed in Product/Trader)
        public virtual ApplicationUser? User { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Order? Order { get; set; }
    }
}
