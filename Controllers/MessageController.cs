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
    [Authorize]
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
                // Get current user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return View(new List<Message>());
                }

                // Get current user with their trader profile
                var userWithTrader = await _context.Users
                    .Include(u => u.Trader)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

            

                // Get messages where current user is sender OR current trader is receiver
                var userId = user.Id;
                var traderId = userWithTrader.Trader.TraderId;
                
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
                // Return empty list if database error
                return View(new List<Message>());
            }
        }

        public async Task<IActionResult> Send(int? receiverId)
        {
            var model = new SendMessageViewModel();
            
            if (receiverId.HasValue)
            {
                model.ReceiverId = receiverId.Value;
                
                // Get receiver name for display
                var receiver = await _context.Traders.FindAsync(receiverId.Value);
                if (receiver != null)
                {
                    model.ReceiverName = receiver.Name;
                }
            }
            
           
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);
            
            // Get list of all available recipients
            var traders = await _context.Traders
                .Where(t => currentUserWithTrader.Trader == null || t.TraderId != currentUserWithTrader.Trader.TraderId)
                .Select(t => new TraderSelectionDto { TraderId = t.TraderId, Name = t.Name })
                .ToListAsync();
            
            ViewBag.Traders = traders;
            ViewBag.CurrentUserIsTrader = currentUserWithTrader?.Trader != null;
            
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

                    // Check if user is trying to message themselves (if they are the trader)
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
        
        private async Task<IActionResult> ReturnSendViewWithData(SendMessageViewModel model)
        {
            // Repopulate ViewBag.Traders for the form
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);
            
            var traders = await _context.Traders
                .Where(t => currentUserWithTrader.Trader == null || t.TraderId != currentUserWithTrader.Trader.TraderId)
                .Select(t => new TraderSelectionDto { TraderId = t.TraderId, Name = t.Name })
                .ToListAsync();
            
            ViewBag.Traders = traders;
            
            // If we have a receiver ID, get the receiver name
            if (model.ReceiverId > 0)
            {
                var receiver = await _context.Traders.FindAsync(model.ReceiverId);
                if (receiver != null)
                {
                    model.ReceiverName = receiver.Name;
                }
            }
            
            return View(model);
        }

        public async Task<IActionResult> Reply(int messageId)
        {
            // Get the original message
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
            if (currentUser == null)
            {
                TempData["Error"] = "Please log in to reply.";
                return RedirectToAction("Index");
            }
            
            // Check if current user is the receiver of the original message
            var currentUserWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);
                
            if (currentUserWithTrader?.Trader == null || currentUserWithTrader.Trader.TraderId != originalMessage.ReceiverId)
            {
                TempData["Error"] = "You can only reply to messages sent to you.";
                return RedirectToAction("Index");
            }
            
            // For reply: Get the original sender info
            var originalSender = await _context.Users.FindAsync(originalMessage.SenderId);
            if (originalSender == null)
            {
                TempData["Error"] = "Original sender not found.";
                return RedirectToAction("Index");
            }
            
            // Check if original sender has a trader profile, if not create a temporary one
            var originalSenderWithTrader = await _context.Users
                .Include(u => u.Trader)
                .FirstOrDefaultAsync(u => u.Id == originalMessage.SenderId);
                
            int targetTraderId;
            string targetName;
            
            if (originalSenderWithTrader?.Trader == null)
            {
                // Create a temporary trader profile for the original sender so they can receive the reply
                var tempTrader = new Trader
                {
                    Name = originalSender.FullName ?? originalSender.Email ?? "User",
                    Email = originalSender.Email ?? "",
                    Phone = "000-000-0000",
                    CIN = "TEMP-" + DateTime.Now.Ticks,
                    GSTNo = "TEMP-GST", 
                    ISO = "TEMP",
                    TradeRole = "Individual",
                    Country = "Unknown",
                    State = "Unknown",
                    City = "Unknown",
                    Address = "Unknown",
                    UserId = originalSender.Id,
                    RegistrationDate = DateTime.UtcNow,
                    Turnover = 0,
                    TrustScore = 0
                };
                
                _context.Traders.Add(tempTrader);
                await _context.SaveChangesAsync();
                targetTraderId = tempTrader.TraderId;
                targetName = tempTrader.Name;
            }
            else
            {
                targetTraderId = originalSenderWithTrader.Trader.TraderId;
                targetName = originalSenderWithTrader.Trader.Name;
            }
            
            // Create a reply message
            var model = new SendMessageViewModel
            {
                ReceiverId = targetTraderId,
                ReceiverName = targetName,
                Content = $"Reply to: \"{originalMessage.Content.Substring(0, Math.Min(50, originalMessage.Content.Length))}...\"\n\n"
            };
            
            ViewBag.OriginalMessage = originalMessage;
            ViewBag.IsReply = true;
            ViewBag.Traders = new List<TraderSelectionDto> 
            { 
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
