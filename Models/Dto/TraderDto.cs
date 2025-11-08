using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSphere3.Models.Dto
{
    public class TraderDto
    {
        [Key]
        public int TraderId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        public int TrustScore { get; set; }

        [StringLength(50)]
        public string TradeRole { get; set; }
    }
}