//using System;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using TradeSphere3.Data;
//using TradeSphere3.Hubs;
//using TradeSphere3.Models;
//using TradeSphere3.Models.Dto;
//using TradeSphere3.Repositories;

//namespace TradeSphere3.Controllers
//{
//    [Authorize]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ChatController : ControllerBase
//    {
//        private readonly IMessageRepository _repo;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly ApplicationDbContext _context;
//        private readonly IHubContext<ChatHub> _hubContext;

//        public ChatController(IMessageRepository repo,
//                              UserManager<ApplicationUser> userManager,
//                              ApplicationDbContext context,
//                              IHubContext<ChatHub> hubContext)
//        {
//            _repo = repo;
//            _userManager = userManager;
//            _context = context;
//            _hubContext = hubContext;
//        }

//        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

//        private async Task<int?> GetCurrentTraderIdAsync()
//        {
//            var uid = CurrentUserId;
//            var trader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == uid);
//            return trader?.TraderId;
//        }

//        // GET: api/chat/contacts
//        [HttpGet("contacts")]
//        public async Task<IActionResult> GetContacts([FromQuery] string? q = null)
//        {
//            var traderId = await GetCurrentTraderIdAsync();
//            if (traderId == null) return Unauthorized();

//            var contacts = await _repo.GetRecentContactsAsync(traderId.Value);

//            if (!string.IsNullOrWhiteSpace(q))
//            {
//                contacts = contacts.Where(c => (c.TraderName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
//            }

//            return Ok(contacts);
//        }

//        // GET: api/chat/traders
//        [HttpGet("traders")]
//        public async Task<IActionResult> GetTraders([FromQuery] string? search = null, [FromQuery]int limit = 50)
//        {
//            var traderId = await GetCurrentTraderIdAsync();
//            if (traderId == null) return Unauthorized();

//            var traders = await _repo.SearchTradersAsync(traderId.Value, search, limit);
//            return Ok(traders.Select(t => new {
//                t.TraderId,
//                Name = t.Name,
//                t.Email,
//                t.City,
//                t.Country
//            }));
//        }

//        // GET: api/chat/conversation/{otherTraderId}
//        [HttpGet("conversation/{otherTraderId}")]
//        public async Task<IActionResult> GetConversation(int otherTraderId, [FromQuery]int page = 0, [FromQuery]int pageSize = 50)
//        {
//            var traderId = await GetCurrentTraderIdAsync();
//            if (traderId == null) return Unauthorized();

//            var (messages, total) = await _repo.GetConversationAsync(traderId.Value, otherTraderId, page, pageSize);

//            // Return messages in chronological order (oldest first)
//            var ordered = messages.OrderBy(m => m.SentAt);

//            // Mark incoming messages as read
//            await _repo.MarkMessagesAsReadAsync(traderId.Value, otherTraderId);

//            return Ok(new {
//                Total = total,
//                Page = page,
//                PageSize = pageSize,
//                Messages = ordered
//            });
//        }

//        // POST: api/chat/send
//        [HttpPost("send")]
//        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto dto)
//        {
//            if (dto == null || string.IsNullOrWhiteSpace(dto.Content)) 
//                return BadRequest("Invalid message.");

//            var userId = CurrentUserId;
//            var traderId = await GetCurrentTraderIdAsync();
//            if (traderId == null) return Unauthorized();

//            // Cannot send to self
//            if (dto.ReceiverTraderId == traderId.Value)
//                return BadRequest("Cannot send message to yourself.");

//            var sent = await _repo.SendMessageAsync(userId, dto);

//            // Broadcast via SignalR
//            await _hubContext.Clients.Group($"Trader_{dto.ReceiverTraderId}")
//                .SendAsync("ReceiveMessage", new {
//                    sent.MessageId,
//                    sent.SenderId,
//                    sent.SenderName,
//                    sent.ReceiverTraderId,
//                    sent.ReceiverName,
//                    sent.Content,
//                    SentAt = sent.SentAt.ToString("yyyy-MM-dd HH:mm:ss"),
//                    sent.EditedAt,
//                    sent.ReplyToMessageId
//                });

//            // Also notify sender
//            await _hubContext.Clients.User(userId)
//                .SendAsync("MessageSent", sent);

//            return Ok(sent);
//        }

//        // PUT: api/chat/edit
//        [HttpPut("edit")]
//        public async Task<IActionResult> EditMessage([FromBody] EditMessageDto dto)
//        {
//            if (dto == null) return BadRequest();

//            var userId = CurrentUserId;
//            var ok = await _repo.EditMessageAsync(userId, dto);
//            if (!ok) return Forbid();

//            // Get message details for broadcast
//            var messageDto = await _repo.GetMessageByIdAsync(dto.MessageId);

//            // Broadcast update
//            if (messageDto != null)
//            {
//                await _hubContext.Clients.Group($"Trader_{messageDto.ReceiverTraderId}")
//                    .SendAsync("MessageUpdated", new {
//                        messageDto.MessageId,
//                        NewContent = dto.NewContent,
//                        messageDto.EditedAt
//                    });
//            }

//            return NoContent();
//        }

//        // DELETE: api/chat/delete/{messageId}
//        [HttpDelete("delete/{messageId}")]
//        public async Task<IActionResult> DeleteMessage(int messageId)
//        {
//            var userId = CurrentUserId;
//            var ok = await _repo.SoftDeleteMessageAsync(userId, messageId);
//            if (!ok) return Forbid();

//            // Get message details for broadcast
//            var messageDto = await _repo.GetMessageByIdAsync(messageId);

//            // Broadcast delete
//            if (messageDto != null)
//            {
//                // Notify receiver
//                await _hubContext.Clients.Group($"Trader_{messageDto.ReceiverTraderId}")
//                    .SendAsync("MessageDeleted", new {
//                        messageId,
//                        ReceiverTraderId = messageDto.ReceiverTraderId
//                    });

//                // Notify sender
//                await _hubContext.Clients.User(messageDto.SenderId)
//                    .SendAsync("MessageDeleted", new {
//                        messageId,
//                        ReceiverTraderId = messageDto.ReceiverTraderId
//                    });
//            }

//            return NoContent();
//        }

//        // POST: api/chat/mark-read/{otherTraderId}
//        [HttpPost("mark-read/{otherTraderId}")]
//        public async Task<IActionResult> MarkRead(int otherTraderId)
//        {
//            var traderId = await GetCurrentTraderIdAsync();
//            if (traderId == null) return Unauthorized();

//            var count = await _repo.MarkMessagesAsReadAsync(traderId.Value, otherTraderId);

//            // Broadcast read receipts
//            await _hubContext.Clients.Group($"Trader_{otherTraderId}")
//                .SendAsync("MessagesRead", new { 
//                    By = traderId.Value, 
//                    Count = count,
//                    When = DateTime.UtcNow.ToString()
//                });

//            return Ok(new { count });
//        }
//    }
//}


































using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TradeSphere3.Data;
using TradeSphere3.Hubs;
using TradeSphere3.Models;
using TradeSphere3.Models.Dto;
using TradeSphere3.Repositories;

namespace TradeSphere3.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IMessageRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IMessageRepository repo,
                              UserManager<ApplicationUser> userManager,
                              ApplicationDbContext context,
                              IHubContext<ChatHub> hubContext)
        {
            _repo = repo;
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private async Task<int?> GetCurrentTraderIdAsync()
        {
            var uid = CurrentUserId;
            var trader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == uid);
            return trader?.TraderId;
        }

        // GET: api/chat/contacts
        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts([FromQuery] string? q = null)
        {
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();

            var contacts = await _repo.GetRecentContactsAsync(traderId.Value);
            
            if (!string.IsNullOrWhiteSpace(q))
            {
                contacts = contacts.Where(c => (c.TraderName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(contacts);
        }

        // GET: api/chat/traders
        [HttpGet("traders")]
        public async Task<IActionResult> GetTraders([FromQuery] string? search = null, [FromQuery] int limit = 50)
        {
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();

            var traders = await _repo.SearchTradersAsync(traderId.Value, search, limit);
            return Ok(traders.Select(t => new {
                t.TraderId,
                Name = t.Name,
                t.Email,
                t.City,
                t.Country
            }));
        }

        // GET: api/chat/new-traders
        [HttpGet("new-traders")]
        public async Task<IActionResult> GetNewTraders([FromQuery] string? search = null, [FromQuery] string? city = null, [FromQuery] string? country = null, [FromQuery] string? role = null)
        {
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();
            
            var traders = await _repo.GetNewTradersForChatAsync(traderId.Value);
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                traders = traders.Where(t => 
                    t.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                    t.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            if (!string.IsNullOrWhiteSpace(city))
            {
                traders = traders.Where(t => 
                    t.City != null && t.City.Contains(city, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            if (!string.IsNullOrWhiteSpace(country))
            {
                traders = traders.Where(t => 
                    t.Country != null && t.Country.Contains(country, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            if (!string.IsNullOrWhiteSpace(role))
            {
                traders = traders.Where(t => 
                    t.TradeRole != null && t.TradeRole.Equals(role, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            return Ok(traders);
        }


        // GET: api/chat/conversation/{otherTraderId}
        [HttpGet("conversation/{otherTraderId}")]
        public async Task<IActionResult> GetConversation(int otherTraderId, [FromQuery] int page = 0, [FromQuery] int pageSize = 50)
        {
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();

            var (messages, total) = await _repo.GetConversationAsync(traderId.Value, otherTraderId, page, pageSize);

            // Return messages in chronological order (oldest first)
            var ordered = messages.OrderBy(m => m.SentAt);

            // Mark incoming messages as read
            await _repo.MarkMessagesAsReadAsync(traderId.Value, otherTraderId);

            return Ok(new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Messages = ordered
            });
        }

        // POST: api/chat/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Invalid message.");

            var userId = CurrentUserId;
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();
            if (dto.ReceiverTraderId == traderId.Value)
                return BadRequest("Cannot send message to yourself.");

            var sent = await _repo.SendMessageAsync(userId, dto);
            if (sent == null) return BadRequest();

            // Create message payload for SignalR
            var messagePayload = new
            {
                sent.MessageId,
                sent.SenderId,
                sent.SenderName,
                sent.ReceiverTraderId,
                sent.ReceiverName,
                sent.Content,
                SentAt = sent.SentAt.ToString("yyyy-MM-dd HH:mm:ss"),
                sent.EditedAt,
                sent.ReplyToMessageId,
                SenderTraderId = traderId.Value
            };

            // Broadcast via SignalR to receiver and update UI
            await _hubContext.Clients.Group($"Trader_{dto.ReceiverTraderId}")
                .SendAsync("ReceiveMessage", messagePayload);
            await _hubContext.Clients.Group($"Trader_{dto.ReceiverTraderId}")
                .SendAsync("UnreadCountUpdated");
            await _hubContext.Clients.All.SendAsync("UpdateChatBoard");

            // Notify sender
            await _hubContext.Clients.User(userId)
                .SendAsync("MessageSent", sent);

            return Ok(sent);
        }

        // PUT: api/chat/edit
        [HttpPut("edit")]
        public async Task<IActionResult> EditMessage([FromBody] EditMessageDto dto)
        {
            if (dto == null) return BadRequest();

            var userId = CurrentUserId;
            var success = await _repo.EditMessageAsync(userId, dto);
            if (!success) return Forbid();

            var messageDto = await _repo.GetMessageByIdAsync(dto.MessageId);
            if (messageDto != null)
            {
                await _hubContext.Clients.Group($"Trader_{messageDto.ReceiverTraderId}")
                    .SendAsync("MessageUpdated", new
                    {
                        messageDto.MessageId,
                        NewContent = dto.NewContent,
                        messageDto.EditedAt
                    });
            }

            return NoContent();
        }

        // DELETE: api/chat/delete/{messageId}
        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = CurrentUserId;
            var success = await _repo.SoftDeleteMessageAsync(userId, messageId);
            if (!success) return Forbid();

            var messageDto = await _repo.GetMessageByIdAsync(messageId);
            if (messageDto != null)
            {
                var deletePayload = new { messageId, ReceiverTraderId = messageDto.ReceiverTraderId };
                
                // Notify receiver
                await _hubContext.Clients.Group($"Trader_{messageDto.ReceiverTraderId}")
                    .SendAsync("MessageDeleted", deletePayload);

                // Notify sender
                await _hubContext.Clients.User(messageDto.SenderId)
                    .SendAsync("MessageDeleted", deletePayload);
            }

            return NoContent();
        }

        // POST: api/chat/mark-read/{otherTraderId}
        [HttpPost("mark-read/{otherTraderId}")]
        public async Task<IActionResult> MarkRead(int otherTraderId)
        {
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();

            var count = await _repo.MarkMessagesAsReadAsync(traderId.Value, otherTraderId);

            // Broadcast read receipts
            await _hubContext.Clients.Group($"Trader_{otherTraderId}")
                .SendAsync("MessagesRead", new
                {
                    By = traderId.Value,
                    Count = count,
                    When = DateTime.UtcNow.ToString()
                });

            // Update unread count for current user
            await _hubContext.Clients.Group($"Trader_{traderId.Value}")
                .SendAsync("UnreadCountUpdated");

            return Ok(new { count });
        }

        // GET: api/chat/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();

            var unreadCount = await _context.Messages
                .Where(m => m.ReceiverTraderId == traderId && m.ReadAt == null)
                .CountAsync();

            return Ok(new { unreadCount });
        }

        // GET: api/chat/chat-board-partial
        [HttpGet("chat-board-partial")]
        public async Task<IActionResult> GetChatBoardPartial()
        {
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null) return Unauthorized();

            var contacts = await _repo.GetRecentContactsAsync(traderId.Value);
            return PartialView("_ChatBoardList", contacts);
        }

        // POST: Chat/SendMessage (for form submission)
        [HttpPost]
        public async Task<IActionResult> SendMessage(int receiverTraderId, string content)
        {
            var userId = CurrentUserId;
            var traderId = await GetCurrentTraderIdAsync();
            if (traderId == null || receiverTraderId == traderId.Value)
                return BadRequest("Cannot send message to yourself or invalid trader.");

            // Create and send message
            var dto = new CreateMessageDto { ReceiverTraderId = receiverTraderId, Content = content };
            var sent = await _repo.SendMessageAsync(userId, dto);
            if (sent == null) return BadRequest();

            // Create message payload
            var messagePayload = new
            {
                sent.MessageId,
                sent.SenderId,
                sent.SenderName,
                sent.ReceiverTraderId,
                sent.ReceiverName,
                sent.Content,
                SentAt = sent.SentAt.ToString("yyyy-MM-dd HH:mm:ss"),
                sent.EditedAt,
                sent.ReplyToMessageId,
                SenderTraderId = traderId.Value
            };

            // Broadcast all updates in parallel
            await Task.WhenAll(
                _hubContext.Clients.Group($"Trader_{receiverTraderId}").SendAsync("ReceiveMessage", messagePayload),
                _hubContext.Clients.Group($"Trader_{receiverTraderId}").SendAsync("UnreadCountUpdated"),
                _hubContext.Clients.All.SendAsync("UpdateChatBoard"),
                _hubContext.Clients.User(userId).SendAsync("MessageSent", sent)
            );

            return Ok();
        }

        // GET: Chat/NewChatList
        [HttpGet]
        public async Task<IActionResult> NewChatList()
        {
            var currentTraderId = await GetCurrentTraderIdAsync();
            if (currentTraderId == null) 
                return Unauthorized();
            
            var traders = await _repo.GetNewTradersForChatAsync(currentTraderId.Value);
            return View(traders);
        }

        // GET: Chat/StartChat
        [HttpGet]
        public IActionResult StartChat(int traderId)
        {
            return Redirect($"/ChatView/Conversation?traderId={traderId}");
        }
    }    
}
