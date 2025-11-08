using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeSphere3.Data;
using TradeSphere3.Models;
using TradeSphere3.Models.Dto;
using TradeSphere3.DTOs;

namespace TradeSphere3.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ContactDto>> GetRecentContactsAsync(int traderId, int limit = 50, string? nameSearch = null, string? cinSearch = null, string? gstSearch = null)
        {
            // Get all messages sent by or received by the trader
            var sent = _context.Messages
                .Where(m => !m.IsDeleted && m.Sender != null && m.Sender.Trader != null && m.Sender.Trader.TraderId == traderId)
                .Select(m => new
                {
                    OtherTraderId = m.ReceiverTraderId,
                    m.Content,
                    m.SentAt,
                    m.ReadAt,
                    m.Status,
                    IsUnread = false // Sent messages are never considered unread for the sender
                });

            var received = _context.Messages
                .Where(m => !m.IsDeleted && m.ReceiverTraderId == traderId && m.Sender != null && m.Sender.Trader != null)
                .Select(m => new
                {
                    OtherTraderId = m.Sender.Trader.TraderId,
                    m.Content,
                    m.SentAt,
                    m.ReadAt,
                    m.Status,
                    IsUnread = m.ReadAt == null
                });

            // Get all data first, then do in-memory grouping
            var allMessages = await sent.Union(received).ToListAsync();
            
            var grouped = allMessages
                .GroupBy(x => x.OtherTraderId)
                .Select(g => new 
                {
                    TraderId = g.Key,
                    LastSentAt = g.Max(x => x.SentAt),
                    LastContent = g.OrderByDescending(x => x.SentAt).Select(x => x.Content).FirstOrDefault(),
                    UnreadCount = g.Count(x => x.IsUnread)
                })
                .ToList(); // Don't order yet - we'll apply ranked search

            // Get trader details efficiently
            var traderIds = grouped.Select(u => u.TraderId).ToList();
            var traderDetails = await _context.Traders
                .Where(t => traderIds.Contains(t.TraderId))
                .ToDictionaryAsync(t => t.TraderId, t => t);

            // Apply ranked search if search terms are provided
            if (!string.IsNullOrEmpty(nameSearch) || !string.IsNullOrEmpty(cinSearch) || !string.IsNullOrEmpty(gstSearch))
            {
                var rankedGroups = grouped.Select(g => 
                {
                    var trader = traderDetails.GetValueOrDefault(g.TraderId);
                    int score = 0;
                    
                    // GST exact match gets highest priority (2)
                    if (!string.IsNullOrEmpty(gstSearch) && !string.IsNullOrEmpty(trader?.GSTNo) && trader.GSTNo.Equals(gstSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = 2;
                    }
                    // CIN exact match gets priority (1)
                    else if (!string.IsNullOrEmpty(cinSearch) && !string.IsNullOrEmpty(trader?.CIN) && trader.CIN.Equals(cinSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = 1;
                    }
                    // GST prefix match gets score based on length
                    else if (!string.IsNullOrEmpty(gstSearch) && !string.IsNullOrEmpty(trader?.GSTNo) && trader.GSTNo.StartsWith(gstSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = gstSearch.Trim().Length; // Longer matches get higher score
                    }
                    // CIN prefix match gets score based on length
                    else if (!string.IsNullOrEmpty(cinSearch) && !string.IsNullOrEmpty(trader?.CIN) && trader.CIN.StartsWith(cinSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = cinSearch.Trim().Length / 2; // Lower weight for CIN
                    }
                    // Name substring match
                    else if (!string.IsNullOrEmpty(nameSearch) && !string.IsNullOrEmpty(trader?.Name))
                    {
                        var nameIndex = trader.Name.IndexOf(nameSearch.Trim(), StringComparison.OrdinalIgnoreCase);
                        if (nameIndex >= 0)
                        {
                            // Lower index means match occurs earlier in the name = higher relevance
                            score = -(nameIndex + 1); // Negative values so they sort below exact/prefix matches
                        }
                    }
                    
                    return new
                    {
                        g.TraderId,
                        g.LastSentAt,
                        g.LastContent,
                        g.UnreadCount,
                        Score = score
                    };
                })
                .OrderByDescending(x => x.Score) // Higher scores first
                .ThenByDescending(x => x.LastSentAt) // Keep recent ordering when scores equal
                .Take(limit)
                .ToList();
                
                // Convert back to proper format
                grouped = rankedGroups.Select(x => new 
                {
                    TraderId = x.TraderId,
                    LastSentAt = x.LastSentAt,
                    LastContent = x.LastContent,
                    UnreadCount = x.UnreadCount
                }).ToList();
            }
            else
            {
                // Normal ordering when no search
                grouped = grouped
                    .OrderByDescending(x => x.LastSentAt)
                    .Take(limit)
                    .ToList();
            }

            var traderIdToUserIdMap = await _context.Traders
                .Where(t => traderIds.Contains(t.TraderId) && t.UserId != null)
                .ToDictionaryAsync(t => t.TraderId, t => t.UserId!);

            // Build result list efficiently
            var result = grouped.Select(contact => {
                var trader = traderDetails.GetValueOrDefault(contact.TraderId);
                return new ContactDto
                {
                    TraderId = contact.TraderId,
                    TraderName = trader?.Name,
                    CIN = trader?.CIN,
                    GSTNo = trader?.GSTNo,
                    Email = trader?.Email,
                    Phone = trader?.Phone,
                    City = trader?.City,
                    Country = trader?.Country,
                    TradeRole = trader?.TradeRole,
                    JoinedDate = trader?.RegistrationDate,
                    LastMessageSnippet = contact.LastContent?.Length > 50 ? 
                        contact.LastContent.Substring(0, 50) + "..." : contact.LastContent,
                    LastMessageAt = contact.LastSentAt,
                    UnreadCount = contact.UnreadCount,
                    IsOnline = traderIdToUserIdMap.TryGetValue(contact.TraderId, out var userId) && 
                              TradeSphere3.Hubs.ChatHub.IsUserOnline(userId)
                };
            }).ToList();

            return result;
        }

        public async Task<(IEnumerable<MessageDto> messages, int total)> GetConversationAsync(int traderIdA, int traderIdB, int page = 0, int pageSize = 50)
        {
            var query = _context.Messages
                .Where(m => !m.IsDeleted &&
                            ((m.ReceiverTraderId == traderIdA && m.Sender != null && m.Sender.Trader != null && m.Sender.Trader.TraderId == traderIdB) ||
                             (m.ReceiverTraderId == traderIdB && m.Sender != null && m.Sender.Trader != null && m.Sender.Trader.TraderId == traderIdA)))
                .OrderByDescending(m => m.SentAt)
                .AsNoTracking();

            var total = await query.CountAsync();

            var messages = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    SenderId = m.SenderId,
                    ReceiverTraderId = m.ReceiverTraderId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    DeliveredAt = m.DeliveredAt,
                    ReadAt = m.ReadAt,
                    IsDeleted = m.IsDeleted,
                    EditedAt = m.EditedAt,
                    ReplyToMessageId = m.ReplyToMessageId,
                    SenderName = m.Sender != null ? m.Sender.Trader != null ? m.Sender.Trader.Name : m.Sender.UserName : null,
                    ReceiverName = m.ReceiverTrader != null ? m.ReceiverTrader.Name : null
                })
                .ToListAsync();

            return (messages.OrderBy(m => m.SentAt), total);
        }

        public async Task<MessageDto?> GetMessageByIdAsync(int messageId)
        {
            var m = await _context.Messages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MessageId == messageId && !x.IsDeleted);

            if (m == null) return null;

            return new MessageDto
            {
                MessageId = m.MessageId,
                SenderId = m.SenderId,
                ReceiverTraderId = m.ReceiverTraderId,
                Content = m.Content,
                SentAt = m.SentAt,
                DeliveredAt = m.DeliveredAt,
                ReadAt = m.ReadAt,
                IsDeleted = m.IsDeleted,
                EditedAt = m.EditedAt,
                ReplyToMessageId = m.ReplyToMessageId,
                SenderName = m.Sender != null ? m.Sender.Trader != null ? m.Sender.Trader.Name : m.Sender.UserName : null,
                ReceiverName = m.ReceiverTrader != null ? m.ReceiverTrader.Name : null
            };
        }

        public async Task<MessageDto> SendMessageAsync(string senderUserId, CreateMessageDto dto)
        {
            var message = new Message
            {
                SenderId = senderUserId,
                ReceiverTraderId = dto.ReceiverTraderId,
                Content = dto.Content,
                SentAt = DateTime.UtcNow,
                Status = "Unread",
                ReplyToMessageId = dto.ReplyToMessageId,
                DeliveredAt = DateTime.UtcNow,
                IsRead = false // always false on send
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return new MessageDto
            {
                MessageId = message.MessageId,
                SenderId = message.SenderId,
                ReceiverTraderId = message.ReceiverTraderId,
                Content = message.Content,
                SentAt = message.SentAt,
                IsDeleted = message.IsDeleted,
                ReplyToMessageId = message.ReplyToMessageId,
                DeliveredAt = message.DeliveredAt
            };
        }

        public async Task<bool> EditMessageAsync(string senderUserId, EditMessageDto dto)
        {
            var m = await _context.Messages.FirstOrDefaultAsync(x => x.MessageId == dto.MessageId && !x.IsDeleted);
            if (m == null) return false;
            if (m.SenderId != senderUserId) return false;

            m.Content = dto.NewContent;
            m.EditedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteMessageAsync(string userId, int messageId)
        {
            var m = await _context.Messages.FirstOrDefaultAsync(x => x.MessageId == messageId);
            if (m == null) return false;

            var allowed = m.SenderId == userId;
            if (!allowed)
            {
                var trader = await _context.Traders.FirstOrDefaultAsync(t => t.UserId == userId);
                if (trader != null && trader.TraderId == m.ReceiverTraderId) allowed = true;
            }

            if (!allowed) return false;

            m.IsDeleted = true;
            m.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkMessagesAsReadAsync(int readerTraderId, int otherTraderId)
        {
            // Only mark messages where the current trader is the receiver
            var unreadMessages = await _context.Messages
                .Where(m => !m.IsDeleted && m.ReceiverTraderId == readerTraderId &&
                            m.Sender != null && m.Sender.Trader != null && m.Sender.Trader.TraderId == otherTraderId && m.ReadAt == null)
                .ToListAsync();

            foreach (var m in unreadMessages)
            {
                m.ReadAt = DateTime.UtcNow;
                m.Status = "Read";
            }

            await _context.SaveChangesAsync();
            return unreadMessages.Count;
        }

        public async Task<IEnumerable<Trader>> SearchTradersAsync(int excludeTraderId, string? query, int limit = 50)
        {
            var q = _context.Traders.AsQueryable().Where(t => t.TraderId != excludeTraderId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                q = q.Where(t => t.Name.Contains(query) || t.Email.Contains(query));
            }

            return await q.Take(limit).ToListAsync();
        }

 

        public async Task<List<TradeSphere3.DTOs.TraderDto>> GetNewTradersForChatAsync(int currentTraderId, string? nameSearch = null, string? cinSearch = null, string? gstSearch = null)
        {
            // Get the current trader's User ID
            var currentTrader = await _context.Traders
                .FirstOrDefaultAsync(t => t.TraderId == currentTraderId);
            
            if (currentTrader?.UserId == null)
                return new List<TradeSphere3.DTOs.TraderDto>();

            // Find all traders the current trader has already chatted with
            var sentMessages = await _context.Messages
                .Where(m => !m.IsDeleted && m.SenderId == currentTrader.UserId)
                .Select(m => m.ReceiverTraderId)
                .Distinct()
                .ToListAsync();
            
            var receivedMessages = await _context.Messages
                .Where(m => !m.IsDeleted && m.ReceiverTraderId == currentTraderId)
                .Select(m => _context.Traders.FirstOrDefault(t => t.UserId == m.SenderId).TraderId)
                .ToListAsync();

            var chattedTraderIds = sentMessages.Concat(receivedMessages).Where(id => id > 0).Distinct().ToList();
            chattedTraderIds.Add(currentTraderId); // Exclude current trader

            // Get traders the current trader has not chatted with and apply ranked search if needed
            var tradersQuery = _context.Traders
                .Where(t => !chattedTraderIds.Contains(t.TraderId) && t.UserId != null);
                
            // Apply filters
            if (!string.IsNullOrEmpty(nameSearch) || !string.IsNullOrEmpty(cinSearch) || !string.IsNullOrEmpty(gstSearch))
            {
                var tradersList = await tradersQuery.ToListAsync();
                var rankedTraders = tradersList.Select(t => 
                {
                    int score = 0;
                    
                    // GST exact match gets highest priority (2)
                    if (!string.IsNullOrEmpty(gstSearch) && !string.IsNullOrEmpty(t.GSTNo) && t.GSTNo.Equals(gstSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = 2;
                    }
                    // CIN exact match gets priority (1)
                    else if (!string.IsNullOrEmpty(cinSearch) && !string.IsNullOrEmpty(t.CIN) && t.CIN.Equals(cinSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = 1;
                    }
                    // GST prefix match gets score based on length
                    else if (!string.IsNullOrEmpty(gstSearch) && !string.IsNullOrEmpty(t.GSTNo) && t.GSTNo.StartsWith(gstSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = gstSearch.Trim().Length; // Longer matches get higher score
                    }
                    // CIN prefix match gets score based on length
                    else if (!string.IsNullOrEmpty(cinSearch) && !string.IsNullOrEmpty(t.CIN) && t.CIN.StartsWith(cinSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        score = cinSearch.Trim().Length / 2; // Lower weight for CIN
                    }
                    // Name substring match
                    else if (!string.IsNullOrEmpty(nameSearch) && !string.IsNullOrEmpty(t.Name))
                    {
                        var nameIndex = t.Name.IndexOf(nameSearch.Trim(), StringComparison.OrdinalIgnoreCase);
                        if (nameIndex >= 0)
                        {
                            // Lower index means match occurs earlier in the name = higher relevance
                            score = -(nameIndex + 1); // Negative values so they sort below exact/prefix matches
                        }
                    }
                    
                    return new
                    {
                        Trader = t,
                        Score = score
                    };
                })
                .OrderByDescending(x => x.Score) // Higher scores first
                .ThenBy(x => x.Trader.Name) // Tie-breaker by name
                .Select(x => new TradeSphere3.DTOs.TraderDto
                {
                    Id = x.Trader.TraderId,
                    Name = x.Trader.Name,
                    Email = x.Trader.Email,
                    Phone = x.Trader.Phone,
                    TradeRole = x.Trader.TradeRole,
                    CIN = x.Trader.CIN,
                    GSTNo = x.Trader.GSTNo,
                    ISO = x.Trader.ISO,
                    Country = x.Trader.Country,
                    State = x.Trader.State,
                    City = x.Trader.City,
                    Address = x.Trader.Address,
                    Turnover = x.Trader.Turnover,
                    RegistrationDate = x.Trader.RegistrationDate,
                    TrustScore = (int)x.Trader.TrustScore
                })
                .ToList();
                    
                return rankedTraders;
            }
            else
            {
                // Normal ordering when no search
                var traderList = await tradersQuery.OrderBy(t => t.Name)
                    .Select(t => new TradeSphere3.DTOs.TraderDto
                    {
                        Id = t.TraderId,
                        Name = t.Name,
                        Email = t.Email,
                        Phone = t.Phone,
                        TradeRole = t.TradeRole,
                        CIN = t.CIN,
                        GSTNo = t.GSTNo,
                        ISO = t.ISO,
                        Country = t.Country,
                        State = t.State,
                        City = t.City,
                        Address = t.Address,
                        Turnover = t.Turnover,
                        RegistrationDate = t.RegistrationDate,
                        TrustScore = (int)t.TrustScore
                    })
                    .ToListAsync();
                    
                return traderList;
            }
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
