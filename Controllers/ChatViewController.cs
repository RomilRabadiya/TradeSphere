//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Threading.Tasks;
//using TradeSphere3.Data;
//using TradeSphere3.Models;
//using TradeSphere3.Models.Dto;
//using TradeSphere3.Repositories;

//namespace TradeSphere3.Controllers
//{
//    [Authorize]
//    public class ChatViewController : Controller
//    {
//        private readonly IMessageRepository _messageRepo;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly ApplicationDbContext _context;

//        public ChatViewController(
//            IMessageRepository messageRepo,
//            UserManager<ApplicationUser> userManager,
//            ApplicationDbContext context)
//        {
//            _messageRepo = messageRepo;
//            _userManager = userManager;
//            _context = context;
//        }

//        // GET: Chat/Chatboard
//        public async Task<IActionResult> Chatboard()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var trader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == user.Id);

//            if (trader == null)
//            {
//                return RedirectToAction("Apply", "Trader");
//            }

//            var contacts = await _messageRepo.GetRecentContactsAsync(trader.TraderId);

//            return View(contacts);
//        }

//        // GET: Chat/Conversation/{traderId}
//        [HttpGet]
//        public async Task<IActionResult> Conversation(int traderId)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var senderTrader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == user.Id);

//            if (senderTrader == null) return RedirectToAction("Apply", "Trader");

//            var receiverTrader = await _context.Traders.FindAsync(traderId);
//            if (receiverTrader == null) return NotFound();

//            ViewBag.ReceiverTraderId = traderId;
//            ViewBag.SenderTraderId = senderTrader.TraderId;
//            ViewBag.CurrentUserId = user.Id;

//            // Pass an empty message model just to satisfy the view requirement
//            return View(new MessageDto());
//        }
//    }
//}


























using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeSphere3.Data;
using TradeSphere3.Models;
using TradeSphere3.Models.Dto;
using TradeSphere3.Repositories;

namespace TradeSphere3.Controllers
{
    [Authorize]
    public class ChatViewController : Controller
    {
        private readonly IMessageRepository _messageRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ChatViewController(
            IMessageRepository messageRepo,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _messageRepo = messageRepo;
            _userManager = userManager;
            _context = context;
        }

        // GET: Chat/Chatboard
        public async Task<IActionResult> Chatboard(string name, string cin, string gstNo)
        {
            var user = await _userManager.GetUserAsync(User);
            var trader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (trader == null)
            {
                return RedirectToAction("Apply", "Trader");
            }

            var contacts = await _messageRepo.GetRecentContactsAsync(trader.TraderId, 50, name, cin, gstNo);

            return View(contacts);
        }

        // GET: Chat/Conversation/{traderId}
        [HttpGet]
        public async Task<IActionResult> Conversation(int traderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var senderTrader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (senderTrader == null) return RedirectToAction("Apply", "Trader");

            var receiverTrader = await _context.Traders.FindAsync(traderId);
            if (receiverTrader == null) return NotFound();

            ViewBag.ReceiverTraderId = traderId;
            ViewBag.SenderTraderId = senderTrader.TraderId;
            ViewBag.CurrentUserId = user.Id;
            ViewBag.ReceiverName = receiverTrader.Name;

            // Pass an empty list to satisfy the view's IEnumerable<MessageDto> model
            return View(System.Linq.Enumerable.Empty<MessageDto>());
        }

        public async Task<IActionResult> NewChatList(string name, string cin, string gstNo)
        {
            var currentTraderId = GetCurrentTraderId();
            var traders = await _messageRepo.GetNewTradersForChatAsync(currentTraderId, name, cin, gstNo);

            return View(traders);
        }

        public IActionResult StartChat(int traderId)
        {
            return Redirect($"/ChatView/Conversation?traderId={traderId}");
        }

        private int GetCurrentTraderId()
        {
            var userId = _userManager.GetUserId(User);
            var trader = _context.Traders.FirstOrDefault(t => t.UserId == userId);
            return trader?.TraderId ?? 0;
        }
    }
}
