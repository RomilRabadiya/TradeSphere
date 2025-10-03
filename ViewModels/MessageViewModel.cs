using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradeSphere3.ViewModels
{
    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public string? SenderName { get; set; }
        public string? ReceiverName { get; set; }
        public string? Content { get; set; }
        public DateTime SentDate { get; set; }
        public string? Status { get; set; }
        public bool IsFromCurrentUser { get; set; }
    }

    public class SendMessageViewModel
    {
        [Required(ErrorMessage = "Receiver is required")]
        public int ReceiverId { get; set; }

        [Required(ErrorMessage = "Message content is required")]
        [StringLength(2000, ErrorMessage = "Message cannot be longer than 2000 characters")]
        public string Content { get; set; } = string.Empty;

        // For display purposes (not required for validation)
        public string? ReceiverName { get; set; }
        
        // For replying to a user (when receiver is the original sender)
        public string? ReplyToUserId { get; set; }
        public bool IsReplyToUser { get; set; } = false;
    }

    public class MessageListViewModel
    {
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public int UnreadCount { get; set; }
        public int TotalMessages { get; set; }
        public string Filter { get; set; } = "All"; // All, Unread, Sent
    }

    public class ConversationViewModel
    {
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
        public SendMessageViewModel NewMessage { get; set; } = new SendMessageViewModel();
        public string? ConversationWith { get; set; }
        public int ConversationWithId { get; set; }
    }
    
    public class TraderSelectionDto
    {
        public int TraderId { get; set; }
        public string? Name { get; set; }
    }
}
