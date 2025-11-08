using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeSphere3.Models;
using TradeSphere3.Data;
using TradeSphere3.Repositories;
using Microsoft.AspNetCore.Identity;

namespace TradeSphere3.Controllers
{
    [Authorize(Roles = "Trader")]
    public class TraderDirectoryController : Controller
    {
        private readonly ITraderRepository _traderRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TraderDirectoryController(ITraderRepository traderRepo, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _traderRepo = traderRepo;
            _context = context;
            _userManager = userManager;
        }

        // GET: /TraderDirectory
        public async Task<IActionResult> Index(string? name, string? cin, string? gstNo)
        {
            var traders = await _traderRepo.SearchAsync(name, cin, gstNo);
            ViewBag.SearchName = name;
            ViewBag.SearchCIN = cin;
            ViewBag.SearchGST = gstNo;
            return View(traders);
        }

        // GET: /TraderDirectory/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var trader = await _traderRepo.GetTraderByIdAsync(id);
            if (trader == null)
                return NotFound();

            // ✅ Calculate average rating from feedback
            var avgRating = await _context.Feedbacks
                .Include(f => f.Product)
                .Where(f => f.Product != null && f.Product.TraderId == id)
                .AverageAsync(f => (double?)f.Rating) ?? 0.0;

            ViewBag.AverageRating = Math.Round(avgRating, 1);

            return View(trader);
        }

        // GET : TraderDirectory/List
        public async Task<IActionResult> List(string? name, string? cin, string? gstNo)
        {
            var traders = await _traderRepo.SearchAsync(name, cin, gstNo);

            // Exclude the currently logged-in trader
            var user = await _userManager.GetUserAsync(User);
            if (user?.Trader != null)
            {
                traders = traders.Where(t => t.TraderId != user.Trader.TraderId);
            }

            return View(traders);
        }


        // GET: /TraderDirectory/Transactions/5
        // GET: /TraderDirectory/Transactions/5
        public async Task<IActionResult> Transactions(int id, string? sortOrder)
        {
            var trader = await _traderRepo.GetTraderByIdAsync(id);
            if (trader == null)
                return NotFound();

            var transactions = _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Where(o => o.Product!.TraderId == id);

            // ✅ Sorting
            transactions = sortOrder switch
            {
                "date_desc" => transactions.OrderByDescending(o => o.OrderDate),
                "date_asc" => transactions.OrderBy(o => o.OrderDate),
                "price_desc" => transactions.OrderByDescending(o => o.Price * o.Quantity),
                "price_asc" => transactions.OrderBy(o => o.Price * o.Quantity),
                _ => transactions.OrderByDescending(o => o.OrderDate) // default
            };

            ViewBag.Trader = trader;
            ViewBag.CurrentSort = sortOrder;

            return View(await transactions.ToListAsync());
        }


    }
}
