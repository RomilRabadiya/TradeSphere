using System;

namespace TradeSphere3.Models.Dto
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; } = null!;
        public int ReceiverTraderId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? EditedAt { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
    }
}