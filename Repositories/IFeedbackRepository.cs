using System.Collections.Generic;
using System.Threading.Tasks;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback> GetByIdAsync(int id);
        Task<IEnumerable<Feedback>> GetByProductAsync(int productId);
        Task<IEnumerable<Feedback>> GetByUserAsync(string userId); // changed from Trader
        Task AddAsync(Feedback feedback);
        Task SaveChangesAsync();
    }
}