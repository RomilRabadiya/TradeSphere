using System.Collections.Generic;
using System.Threading.Tasks;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public interface ITraderRepository
    {
        Task<IEnumerable<Trader>> GetAllTradersAsync();
        Task<Trader> GetTraderByIdAsync(int traderId);
        Task<Trader> GetTraderByUserIdAsync(string userId);
        Task<Trader> AddTraderAsync(Trader trader);
        Task UpdateTraderAsync(Trader trader);
        Task<bool> DeleteTraderAsync(int traderId);
        Task<bool> TraderExistsAsync(int traderId);

        //search management
        Task<IEnumerable<Trader>> SearchAsync(string? name, string? cin, string? gstNo);
        Task<Trader?> GetBasicByIdAsync(int id);
    }
}