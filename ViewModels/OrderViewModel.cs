using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TradeSphere3.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string TraderName { get; set; }
    }

    public class CreateOrderViewModel
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Required]
        [StringLength(300, ErrorMessage = "Address cannot be longer than 300 characters")]
        public string ShippingAddress { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        // For display
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal TotalAmount => Quantity * ProductPrice;
    }

    public class OrderListViewModel
    {
        public List<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
        public string Status { get; set; } = "All";
        public int TotalOrders { get; set; }
        public decimal TotalValue { get; set; }
    }
}
