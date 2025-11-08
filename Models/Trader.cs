using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;


namespace TradeSphere3.Models
{
	public class Trader
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TraderId { get; set; }  // Primary Key

		[Required]
		[StringLength(200)]
		public string Name { get; set; }  // Company/Merchant Name

		[StringLength(50)]
		public string CIN { get; set; }  // Corporate Identity Number

		[StringLength(50)]
		public string GSTNo { get; set; }  // GST Number

		[StringLength(50)]
		public string ISO { get; set; }  // ISO Certification Number

		[Required]
		[StringLength(100)]
		public string Country { get; set; }  // Country

		[StringLength(100)]
		public string State { get; set; }  // State/Region

		[StringLength(100)]
		public string City { get; set; }  // City

		[StringLength(300)]
		public string Address { get; set; }  // Full Address

		[Required]
		[EmailAddress]
        [Remote(action: "IsEmailAvailable", controller: "Trader")]
        public string Email { get; set; }  // Contact Email

		[Required]
		[Phone]
		public string Phone { get; set; }  // Contact Phone

		[Required]
		[StringLength(20)]
		public string TradeRole { get; set; }  // Importer / Exporter / Both

		[DataType(DataType.Date)]
		public DateTime RegistrationDate { get; set; }  // Registration Date

		[DataType(DataType.Date)]
		public DateTime? UpdatedAt { get; set; }  // Last Update Date

		[Column(TypeName = "decimal(18,2)")]
		public decimal Turnover { get; set; }  // Yearly Trade Turnover

		[Column(TypeName = "decimal(5,2)")]
		public decimal TrustScore { get; set; }  // Calculated Trust Score (0–100)

        // Navigation
        public string UserId { get; set; }
		public virtual ApplicationUser User { get; set; }
    }
}