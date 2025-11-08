using System.ComponentModel.DataAnnotations;

namespace TradeSphere3.Models.Dto
{
    public class EditMessageDto
    {
        [Required]
        public int MessageId { get; set; }

        [Required]
        [StringLength(2000)]
        public string NewContent { get; set; } = null!;
    }
}