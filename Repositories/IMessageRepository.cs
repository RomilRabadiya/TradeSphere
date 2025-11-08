using System.Collections.Generic;
using System.Threading.Tasks;
using TradeSphere3.Models;
using TradeSphere3.Models.Dto;
using TradeSphere3.DTOs;

namespace TradeSphere3.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<ContactDto>> GetRecentContactsAsync(int traderId, int limit = 50, string? nameSearch = null, string? cinSearch = null, string? gstSearch = null);
        Task<(IEnumerable<MessageDto> messages, int total)> GetConversationAsync(int traderIdA, int traderIdB, int page = 0, int pageSize = 50);
        Task<MessageDto?> GetMessageByIdAsync(int messageId);
        Task<MessageDto> SendMessageAsync(string senderUserId, CreateMessageDto dto);
        Task<bool> EditMessageAsync(string senderUserId, EditMessageDto dto);
        Task<bool> SoftDeleteMessageAsync(string userId, int messageId);
        Task<int> MarkMessagesAsReadAsync(int readerTraderId, int otherTraderId);
        Task<IEnumerable<Trader>> SearchTradersAsync(int excludeTraderId, string? query, int limit = 50);
        Task<List<TradeSphere3.DTOs.TraderDto>> GetNewTradersForChatAsync(int currentTraderId, string? nameSearch = null, string? cinSearch = null, string? gstSearch = null);
        Task SaveChangesAsync();
    }
}