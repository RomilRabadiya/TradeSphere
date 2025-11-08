using System.ComponentModel.DataAnnotations;

namespace TradeSphere3.Models.Dto
{
    public class CreateMessageDto
    {
        [Required]
        public int ReceiverTraderId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = null!;

        public int? ReplyToMessageId { get; set; }
    }
}