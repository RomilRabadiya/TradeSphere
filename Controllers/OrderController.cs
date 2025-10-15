using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TradeSphere3.Repositories;
using TradeSphere3.Models;
using TradeSphere3.Data;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace TradeSphere3.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public OrderController(IOrderRepository orderRepository, IProductRepository productRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userManager = userManager;
            _context = context;
        }

        // View orders based on user role
        public IActionResult Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Users should only see their own orders - redirect to MyOrders
                if (User.IsInRole("User"))
                {
                    return RedirectToAction("MyOrders");
                }
                
                // Traders can only see orders for their own products
                if (User.IsInRole("Trader"))
                {
                    // Get the trader record for the current user
                    var trader = _context.Traders.FirstOrDefault(t => t.UserId == userId);
                    if (trader != null)
                    {
                        var traderOrders = _orderRepository.GetByTraderId(trader.TraderId);
                        return View(traderOrders);
                    }
                }
                
                // Default: return empty list if no specific role match
                return View(new List<Order>());
            }
            catch (Exception)
            {
                return View(new List<Order>());
            }
        }

        // View my orders (for users)
        public IActionResult MyOrders()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var orders = _orderRepository.GetByUserId(userId);
                return View(orders);
            }
            catch (Exception)
            {
                return View(new List<Order>());
            }
        }

        // Create order
        public IActionResult Create(int productId)
        {
            try
            {
                var product = _productRepository.GetById(productId);
                if (product == null)
                {
                    return NotFound();
                }

                ViewBag.Product = product;
                return View();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Create(int productId, int quantity, string shippingAddress, string paymentMethod)
        {
            try
            {
                var product = _productRepository.GetById(productId);
                if (product == null || product.Quantity < quantity)
                {
                    ModelState.AddModelError("", "Product not available or insufficient stock");
                    ViewBag.Product = product;
                    return View();
                }

                if(product.Quantity == 0)
                {
                    product.Status = "Inactive";
                    ViewBag.status = "Inactive";
                    return View();
                }
                 
              
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var order = new Order
                    {
                        ProductId = productId,
                        TraderId = product.TraderId, //store trader permanently
                        Quantity = quantity,
                        Price = product.Price,
                        ShippingAddress = shippingAddress,
                        PaymentMethod = paymentMethod,
                        UserId = userId,
                        OrderDate = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    _orderRepository.Add(order);

                    // Update product quantity
                    product.Quantity -= quantity;
                    _productRepository.Update(product);

                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction("MyOrders");
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error placing order: " + ex.Message);
                return View();
            }
        }
        

        // Order details
        public IActionResult Details(int id)
        {
            try
            {
                var order = _orderRepository.GetById(id);
                if (order == null)
                {
                    return NotFound();
                }

                // Check if user owns this order (for Users) or allow all for Traders/Admin
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (User.IsInRole("User") && order.UserId != userId)
                {
                    return Forbid();
                }

                return View(order);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
