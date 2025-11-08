using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TradeSphere3.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using TradeSphere3.Data;
using Microsoft.EntityFrameworkCore;

namespace TradeSphere3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        //Is check User role and Check Trader is Complete their Profile or not
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                ViewBag.UserName = user.FullName ?? user.UserName;

                // Get roles directly from Identity
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User"; // Default fallback

                ViewBag.Role = role;
                ViewBag.IsTrader = role == "Trader";


                var userWithTrader = await _context.Users     //Get All User
                    .Include(u => u.Trader)     /// check User is Link with Trader or Not is Trader? Trader Attribute of user
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                //If User is Link with Trader Then Assign That Trader Otherwise Null
                ViewBag.HasTraderProfile = userWithTrader.Trader != null;
            }
            else
            {
                ViewBag.UserName = "Unknown User";
                ViewBag.Role = "No Role Assigned";
                ViewBag.IsTrader = false;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}