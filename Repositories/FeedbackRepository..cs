using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeSphere3.Data;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ApplicationDbContext _context;

        public FeedbackRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> GetByIdAsync(int id)
        {
            return await _context.Feedbacks
                .Include(f => f.Product)
                .Include(f => f.User)  // If navigation property exists
                .FirstOrDefaultAsync(f => f.FeedbackId == id);
        }

        public async Task<IEnumerable<Feedback>> GetByProductAsync(int productId)
        {
            return await _context.Feedbacks
                .Where(f => f.ProductId == productId)
                .Include(f => f.User) // optional
                .OrderByDescending(f => f.ReviewDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Feedback>> GetByUserAsync(string userId)
        {
            return await _context.Feedbacks
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .OrderByDescending(f => f.ReviewDate)
                .ToListAsync();
        }

        public async Task AddAsync(Feedback feedback)
        {
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}