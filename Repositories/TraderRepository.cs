using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeSphere3.Data;
using TradeSphere3.Models;
using Microsoft.EntityFrameworkCore.SqlServer;


//In this Repository we will use Async methods for better scalability and performance

//Get More benifit of Asynchronous programming in C#

//Use FindAsync instead of Find

namespace TradeSphere3.Repositories
{
    public class TraderRepository : ITraderRepository
    {
        private readonly ApplicationDbContext _context;

        public TraderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trader>> GetAllTradersAsync()
        {
            return await _context.Traders.Include(t => t.User).ToListAsync();
        }

        public async Task<Trader> GetTraderByIdAsync(int traderId)
        {
            return await _context.Traders.Include(t => t.User).FirstOrDefaultAsync(t => t.TraderId == traderId);
        }

        public async Task<Trader> GetTraderByUserIdAsync(string userId)
        {
            return await _context.Traders.Include(t => t.User).FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Trader> AddTraderAsync(Trader trader)
        {
            _context.Traders.Add(trader);
            await _context.SaveChangesAsync();
            return trader;
        }

        public async Task UpdateTraderAsync(Trader trader)
        {
            _context.Traders.Update(trader);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteTraderAsync(int traderId)
        {
            var trader = await _context.Traders.FindAsync(traderId);
            if (trader != null)
            {
                _context.Traders.Remove(trader);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> TraderExistsAsync(int traderId)
        {
            return await _context.Traders.AnyAsync(t => t.TraderId == traderId);
        }




        //search management
        // Ranked Search Management
        public async Task<IEnumerable<Trader>> SearchAsync(string? name, string? cin, string? gstNo)
        {
            // Convert Trader Object to Query
            var query = _context.Traders.AsQueryable();

            // Apply filters (constraints)
            if (!string.IsNullOrWhiteSpace(name))
            {
                var n = name.Trim().ToLower();
                query = query.Where(t => EF.Functions.Like(t.Name.ToLower(), $"%{n}%"));//Pattern match with name
            }

            if (!string.IsNullOrWhiteSpace(cin))
            {
                var c = cin.Trim();
                query = query.Where(t => EF.Functions.Like(t.CIN, $"{c}%"));//prefix match with CIN
            }

            if (!string.IsNullOrWhiteSpace(gstNo))
            {
                var g = gstNo.Trim();
                query = query.Where(t => EF.Functions.Like(t.GSTNo, $"{g}%"));//prefix match with GST
            }

            query = query.Where(t => t.CIN != "TEMP-638951421242467023");




            // Ranking calculation
            query = query
                .OrderByDescending(t =>
                    // Highest priority → exact GST match
                    (!string.IsNullOrWhiteSpace(gstNo) && t.GSTNo == gstNo) ? 2 :
                    //Exact GST Match then priority 5
                    // Second priority → exact CIN match
                    (!string.IsNullOrWhiteSpace(cin) && t.CIN == cin) ? 1 :
                    //Exact CIN Match then priority 4
                    // Next → GST prefix length match
                    (!string.IsNullOrWhiteSpace(gstNo) && t.GSTNo.StartsWith(gstNo)) ? gstNo.Length : 0
                )
                .ThenByDescending(t =>
                    // Then → CIN prefix length match
                    (!string.IsNullOrWhiteSpace(cin) && t.CIN.StartsWith(cin)) ? cin.Length : 0
                )
                .ThenBy(t =>
                    // Finally → name match ordering (earlier substring position first)
                    !string.IsNullOrWhiteSpace(name)
                        ? t.Name.ToLower().IndexOf(name.Trim().ToLower())
                        : int.MaxValue
                )
                .ThenBy(t => t.Name); // tie-breaker

            return await query
                .Include(t => t.User)
                .Take(200) // cap
                .ToListAsync();
        }



        public async Task<Trader?> GetBasicByIdAsync(int id)
        {
            return await _context.Traders.FindAsync(id);
        }





        public async Task<bool> DeleteTraderByEmailAsync(string email)
        {
            var trader = await _context.Traders
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Email == email);

            if (trader != null)
            {
                _context.Traders.Remove(trader);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}