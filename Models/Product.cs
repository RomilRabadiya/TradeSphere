using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradeSphere3.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }  // Primary Key

        [Required]
        public int TraderId { get; set; }   // FK to Trader

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }    // Product Name

        [MaxLength(1000)]
        public string Description { get; set; }  // Detailed Description

        [MaxLength(100)]
        public string Category { get; set; }     // Product Category

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }       // Unit Price

        public int Quantity { get; set; }        // Available Stock Quantity

        [MaxLength(50)]
        public string Unit { get; set; }         // Measurement Unit (kg, pcs, liters, etc.)

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;  // Default when added

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Default status

        // Navigation property
        [ForeignKey("TraderId")]
        public virtual Trader Trader { get; set; }

    }


}