using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeSphere3.Repositories;
using TradeSphere3.Models;
using TradeSphere3.ViewModels;
using TradeSphere3.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace TradeSphere3.Controllers
{
    [Authorize(Roles = "Trader")]
    public class MessageController : Controller
    {
        private readonly IMessageRepositry _messageRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MessageController(IMessageRepositry messageRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _messageRepository = messageRepository;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) { return View(new List<Message>()); }

                var userWithTrader = await _context.Users
                    .Include(u => u.Trader)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (userWithTrader?.Trader == null)
                {
                    return View(new List<Message>());
                }

                var traderId = userWithTrader.Trader.TraderId;
                var userId = user.Id;

                var messages = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Receiver)
                    .Where(m => m.SenderId == userId || m.ReceiverId == traderId)
                    .OrderByDescending(m => m.SentDate)
                    .ToListAsync();

                return View(messages);
            }
            catch (Exception)
            {
                return View(new List<Message>());
            }
        }
        // In MessageController.cs
        public async Task<IActionResult> Send(int? receiverId)
        {
            var model = new SendMessageViewModel();

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);


            if (currentUserWithTrader?.Trader == null)
            {
                TempData["Error"] = "Only traders can send messages.";
                return RedirectToAction("Index");
            }

            if (receiverId.HasValue)
            {
                model.ReceiverId = receiverId.Value;
                var receiver = await _context.Traders.FindAsync(receiverId.Value);
                if (receiver != null)
                {
                    model.ReceiverName = receiver.Name;
                }
            }


            var traders = await _context.Traders
                .Where(t => t.TraderId != currentUserWithTrader.Trader.TraderId)
                .Select(t => new TraderSelectionDto { TraderId = t.TraderId, Name = t.Name })
                .ToListAsync();

            ViewBag.Traders = traders;

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Send(SendMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get current user
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "User not found. Please log in again.");
                        return await ReturnSendViewWithData(model);
                    }

                    // Check if receiver exists
                    var receiver = await _context.Traders.FindAsync(model.ReceiverId);
                    if (receiver == null)
                    {
                        ModelState.AddModelError("ReceiverId", "The selected trader does not exist.");
                        return await ReturnSendViewWithData(model);
                    }


                    var userWithTrader = await _context.Users
                        .Include(u => u.Trader)
                        .FirstOrDefaultAsync(u => u.Id == user.Id);

                    if (userWithTrader?.Trader != null && receiver.TraderId == userWithTrader.Trader.TraderId)
                    {
                        ModelState.AddModelError("ReceiverId", "You cannot send a message to yourself.");
                        return await ReturnSendViewWithData(model);
                    }

                    var message = new Message
                    {
                        SenderId = user.Id, // Use ApplicationUser.Id
                        ReceiverId = model.ReceiverId, // Use Trader.TraderId
                        Content = model.Content.Trim(),
                        SentDate = DateTime.UtcNow,
                        Status = "Unread"
                    };

                    _messageRepository.Add(message);
                    TempData["Success"] = $"Message sent successfully to {receiver.Name}!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while sending the message. Please try again.");
                    // Log the exception for debugging
                    Console.WriteLine($"Error sending message: {ex.Message}");
                }
            }

            return await ReturnSendViewWithData(model);
        }

        // In MessageController.cs
        private async Task<IActionResult> ReturnSendViewWithData(SendMessageViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            if (currentUserWithTrader?.Trader == null)
            {
                ViewBag.Traders = new List<TraderSelectionDto>();
                return View("Send", model);
            }


            var traders = await _context.Traders
                .Where(t => t.TraderId != currentUserWithTrader.Trader.TraderId)
                .Select(t => new TraderSelectionDto { TraderId = t.TraderId, Name = t.Name })
                .ToListAsync();

            ViewBag.Traders = traders;


            if (model.ReceiverId > 0)
            {
                var receiver = await _context.Traders.FindAsync(model.ReceiverId);
                if (receiver != null)
                {
                    model.ReceiverName = receiver.Name;
                }
            }

            return View("Send", model);
        }


        public async Task<IActionResult> Reply(int messageId)
        {
            var originalMessage = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);

            if (originalMessage == null)
            {
                TempData["Error"] = "Original message not found.";
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);


            if (currentUserWithTrader?.Trader == null || currentUserWithTrader.Trader.TraderId != originalMessage.ReceiverId)
            {
                TempData["Error"] = "wrong moves.";
                return RedirectToAction("Index");
            }


            var originalSenderWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == originalMessage.SenderId);

            if (originalSenderWithTrader?.Trader == null)
            {
                TempData["Error"] = "You can only reply to other traders.";
                return RedirectToAction("Index");
            }

            // This section is now clean. It no longer creates temporary traders.
            int targetTraderId = originalSenderWithTrader.Trader.TraderId;
            string targetName = originalSenderWithTrader.Trader.Name;

            var model = new SendMessageViewModel
            {
                ReceiverId = targetTraderId,
                ReceiverName = targetName,
                Content = $"Reply to: \"{originalMessage.Content.Substring(0, Math.Min(50, originalMessage.Content.Length))}...\"\n\n"
            };

            ViewBag.IsReply = true;
            ViewBag.Traders = new List<TraderSelectionDto> {
        new TraderSelectionDto { TraderId = targetTraderId, Name = targetName }
    };

            return View("Send", model);
        }

        public IActionResult Details(int id)
        {
            try
            {
                var message = _messageRepository.GetById(id);
                if (message == null)
                {
                    return NotFound();
                }

                // Mark as read
                if (message.Status == "Unread")
                {
                    message.Status = "Read";
                    _messageRepository.Update(message);
                }

                return View(message);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}