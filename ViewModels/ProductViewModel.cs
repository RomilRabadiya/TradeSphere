using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradeSphere3.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public string TraderName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; }

        [StringLength(100, ErrorMessage = "Category cannot be longer than 100 characters")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [StringLength(50, ErrorMessage = "Unit cannot be longer than 50 characters")]
        public string Unit { get; set; } = "pcs";

        public string Status { get; set; } = "Active";
    }

    public class EditProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; }

        [StringLength(100, ErrorMessage = "Category cannot be longer than 100 characters")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [StringLength(50, ErrorMessage = "Unit cannot be longer than 50 characters")]
        public string Unit { get; set; }

        public string Status { get; set; }
    }

    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public string Category { get; set; } = "All";
        public string SearchTerm { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int TotalProducts { get; set; }
    }
}
