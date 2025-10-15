using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TradeSphere3.Repositories;
using TradeSphere3.Models;
using TradeSphere3.ViewModels;
using TradeSphere3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace TradeSphere3.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ITraderRepository _traderRepository;

        public ProductController(IProductRepository productRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context , IFeedbackRepository feedbackRepository, ITraderRepository traderRepository)
        {
            _productRepository = productRepository;
            _userManager = userManager;
            _feedbackRepository = feedbackRepository;
            _context = context;
            _traderRepository = traderRepository;
        }

        // View all products (public access)
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = _productRepository.GetAll();

                // Get logged-in user's id
                var userId = _userManager.GetUserId(User);
                var trader = !string.IsNullOrEmpty(userId)
                    ? await _traderRepository.GetTraderByUserIdAsync(userId)
                    : null;

                ViewBag.CurrentTraderId = trader?.TraderId;

                return View(products);
            }
            catch (Exception)
            {
                return View(new List<Product>());
            }
        }

        // View product details with feedback
        public async Task<IActionResult> Details(int id,string? callingMethods)
        {
            try
            {
                // Get the product
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Get all feedbacks for this product
                //var feedbacks = await _feedbackRepository.GetByProductAsync(id);
                var feedbacks = (await _feedbackRepository.GetByProductAsync(id))
                     .OrderByDescending(f => f.ReviewDate);

                // Create a ViewModel to pass product and feedbacks together
                var viewModel = new ProductDetailsViewModel
                {
                    Product = product,
                    Feedbacks = feedbacks
                };

                ViewBag.CallingMethods = callingMethods;

                return View(viewModel);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // GET: /Product/MyProducts
        // Shows products that belong to the currently logged-in trader
        [Authorize(Roles = "Trader")]
        public async Task<IActionResult> MyProducts()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find trader linked to this user
            var trader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == currentUser.Id);
            if (trader == null)
            {
                // No trader profile yet — return an empty list view
                return View(new List<Product>());
            }

            var products = _productRepository.GetByTraderId(trader.TraderId);
            return View(products);
        }





        // Create product - only for traders
        [Authorize(Roles = "Trader")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Trader")]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get current logged-in user
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser == null)
                    {
                        ModelState.AddModelError("", "User not found. Please log in again.");
                        return View(model);
                    }

                    // Find the trader record associated with this user
                    var trader = await _context.Traders
                        .FirstOrDefaultAsync(t => t.UserId == currentUser.Id);
                    
                    int traderId;
                    if (trader == null)
                    {
                        // If no trader record exists, create one automatically
                        var newTrader = new Trader
                        {
                            UserId = currentUser.Id,
                            Name = currentUser.FullName ?? "Unknown Trader",
                            Email = currentUser.Email,
                            Phone = "Not provided",
                            Address = "Not provided",
                            City = "Not provided",
                            State = "Not provided",
                            Country = "Not provided",
                            // Status = "Active",
                            // Role = "Trader",
                            RegistrationDate = DateTime.UtcNow
                        };
                        
                        _context.Traders.Add(newTrader);
                        await _context.SaveChangesAsync();
                        traderId = newTrader.TraderId;
                        
                        TempData["Info"] = "Trader profile created automatically.";
                    }
                    else
                    {
                        traderId = trader.TraderId;
                    }
                    
                    var product = new Product
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Category = model.Category,
                        Price = model.Price,
                        Quantity = model.Quantity,
                        Unit = model.Unit,
                        Status = model.Status,
                        TraderId = traderId // Use determined trader ID
                    };

                    _productRepository.Add(product);
                    TempData["Success"] = "Product added successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error adding product: " + ex.Message);
                }
            }
            return View(model);
        }

        // Edit product - only for traders (owner)
        [Authorize(Roles = "Trader")]
        public IActionResult Edit(int id)
        {
            try
            {
                var product = _productRepository.GetById(id);
                if (product == null)
                {
                    return NotFound();
                }

                // TODO: Check if current user is the owner of this product
                // if (product.TraderId != currentUserId) return Forbid();

                var model = new EditProductViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Category = product.Category,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    Unit = product.Unit,
                    Status = product.Status
                };

                return View(model);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Trader")]
        public IActionResult Edit(int id, EditProductViewModel model)
        {
            if (id != model.ProductId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = _productRepository.GetById(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    // TODO: Check if current user is the owner
                    // if (product.TraderId != currentUserId) return Forbid();

                    // Update product properties
                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.Category = model.Category;
                    product.Price = model.Price;
                    product.Quantity = model.Quantity;
                    product.Unit = model.Unit;
                    product.Status = model.Status;

                    _productRepository.Update(product);
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating product: " + ex.Message);
                }
            }
            return View(model);
        }


        [HttpPost]
        [Authorize(Roles = "Trader")]
        public async Task<IActionResult> RefillProduct(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            product.Quantity += quantity; // add refill quantity
            _context.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyProducts"); // refresh the product list
        }






        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trader = _context.Traders.FirstOrDefault(t => t.UserId == userId);
            if (trader == null)
            {
                TempData["Error"] = "You are not authorized to delete this product.";
                return RedirectToAction("Index");
            }

            if (product.TraderId != trader.TraderId)
            {
                TempData["Error"] = "You are not authorized to delete this product.";
                return RedirectToAction("Index");
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("MyProducts");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trader = _context.Traders.FirstOrDefault(t => t.UserId == userId);
            if (trader == null)
            {
                TempData["Error"] = "You are not authorized to delete this product.";
                return RedirectToAction("MyProducts");
            }

            if (product.TraderId != trader.TraderId)
            {
                TempData["Error"] = "You are not authorized to delete this product.";
                return RedirectToAction("MyProducts");
            }

            _productRepository.Delete(id);

            TempData["Success"] = "Product and its feedbacks deleted successfully! Orders retained.";
            return RedirectToAction("MyProducts");
        }
    }
}