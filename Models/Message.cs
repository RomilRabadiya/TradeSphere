using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSphere3.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }  // Primary Key

        [Required]
        public string SenderId { get; set; } = string.Empty;   // FK to AspNetUsers (UserId)

        [Required]
        public int ReceiverId { get; set; } // FK to Traders (TraderId)

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty; // Message text

        [Required]
        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Unread"; // Default status

        // Navigation properties for the original direction (User -> Trader)
        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Trader? Receiver { get; set; }
    }
}
