using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSphere3.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public string SenderId { get; set; }

        // Store receiver as TraderId to match existing DB schema (column name ReceiverId)
        [Required]
        [Column("ReceiverId")]
        public int ReceiverTraderId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        // Map to existing column SentDate
        [Column("SentDate")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // DB requires Status (e.g., Unread/Read)
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Unread";

        // Reply functionality - store the ID of the message being replied to
        public int? ReplyToMessageId { get; set; }

        [ForeignKey("ReplyToMessageId")]
        public Message? ReplyToMessage { get; set; }

        // Navigation property for messages that reply to this one
        public ICollection<Message> Replies { get; set; } = new List<Message>();

        [ForeignKey("SenderId")]
        public ApplicationUser? Sender { get; set; }

        [ForeignKey("ReceiverTraderId")]
        public Trader? ReceiverTrader { get; set; }

        // Advanced metadata
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime? EditedAt { get; set; }

        [NotMapped]
        public bool IsRead { get; set; } = false;
    }
}
