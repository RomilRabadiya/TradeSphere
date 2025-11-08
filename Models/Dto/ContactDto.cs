using System;

namespace TradeSphere3.Models.Dto
{
    public class ContactDto
    {
        public int TraderId { get; set; }
        public string? TraderName { get; set; }
        public string? CIN { get; set; }
        public string? GSTNo { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? TradeRole { get; set; }
        public DateTime? JoinedDate { get; set; }
    
        public string? LastMessageSnippet { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }
}
