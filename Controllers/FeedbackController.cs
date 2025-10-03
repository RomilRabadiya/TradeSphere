using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeSphere3.Data;
using TradeSphere3.Models;
using System.Linq;

namespace TradeSphere3.Controllers
{
    [Authorize] // Any logged-in user can leave feedback
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Feedback/Create
        [HttpGet]
        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var model = new Feedback
            {
                ProductId = productId,
                ReviewDate = DateTime.Now
            };

            ViewBag.ProductName = product.Name;
            ViewBag.UserId = (await _userManager.GetUserAsync(User))?.Id;
            return View(model);
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            // Get logged-in identity user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Set required fields not posted in form
            feedback.ReviewDate = DateTime.Now;

            // Validate after setting required values
            if (!ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(feedback.ProductId);
                ViewBag.ProductName = product?.Name ?? "Unknown Product";
                return View(feedback);
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✅ Thank you for your feedback!";
            return RedirectToAction("Details", "Product", new { id = feedback.ProductId });
        }

        // GET: Feedback/Feedback/5
        // View all feedback for a specific trader
        public async Task<IActionResult> Feedback(int traderId, string sortBy = "date", bool desc = true)
        {
            var trader = await _context.Traders
                .FirstOrDefaultAsync(t => t.TraderId == traderId);

            if (trader == null) return NotFound();

            var feedbacks = _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Product)
                .Where(f => f.Product!.TraderId == traderId);

            // Sorting logic
            feedbacks = sortBy switch
            {
                "rating" => desc
                    ? feedbacks.OrderByDescending(f => f.Rating).ThenByDescending(f => f.ReviewDate)
                    : feedbacks.OrderBy(f => f.Rating).ThenBy(f => f.ReviewDate),

                _ => desc
                    ? feedbacks.OrderByDescending(f => f.ReviewDate)
                    : feedbacks.OrderBy(f => f.ReviewDate)
            };

            ViewBag.Trader = trader;
            ViewBag.SortBy = sortBy;
            ViewBag.IsDesc = desc;
            return View(await feedbacks.ToListAsync());
        }

        
        public async Task<IActionResult> ProductFeedback(int productId, string sortBy = "date", bool desc = true)
        {
            var product = await _context.Products
                .Include(p => p.Trader)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) return NotFound();

            var feedbacks = _context.Feedbacks
                .Include(f => f.User)
                .Where(f => f.ProductId == productId);

            // Sorting logic
            feedbacks = sortBy switch
            {
                "rating" => desc
                    ? feedbacks.OrderByDescending(f => f.Rating).ThenByDescending(f => f.ReviewDate)
                    : feedbacks.OrderBy(f => f.Rating).ThenBy(f => f.ReviewDate),

                _ => desc
                    ? feedbacks.OrderByDescending(f => f.ReviewDate)
                    : feedbacks.OrderBy(f => f.ReviewDate)
            };

            ViewBag.Product = product;
            ViewBag.Trader = product.Trader;
            ViewBag.SortBy = sortBy;
            ViewBag.IsDesc = desc;

            ViewBag.ProductFeedback=true; // Indicate this is product-specific feedback view

            return View("Feedback",await feedbacks.ToListAsync());
        }

    }
}