using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeSphere3.Data;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public class ProductRepository : IProductRepository
    {

        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get operations
        public Product GetById(int id)
        {
            return _context.Products
                .Include(p => p.Trader)
                // .Include(p => p.Feedbacks) // Temporarily commented out
                .FirstOrDefault(p => p.ProductId == id);
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Trader)
                // .Include(p => p.Feedbacks) // Temporarily commented out
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public IEnumerable<Product> GetAll()
        {
            return _context.Products
                .Include(p => p.Trader)
                // .Include(p => p.Feedbacks) // Temporarily commented out
                .ToList();
        }

        public IEnumerable<Product> GetByTraderId(int traderId)
        {
            return _context.Products
                // .Include(p => p.Feedbacks) // Temporarily commented out
                .Where(p => p.TraderId == traderId)
                .ToList();
        }

        public IEnumerable<Product> GetByName(string name)
        {
            return _context.Products
                // .Include(p => p.Feedbacks) // Temporarily commented out
                .Where(p => p.Name.Contains(name))
                .ToList();
        }

        public IEnumerable<Product> GetByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _context.Products
                // .Include(p => p.Feedbacks) // Temporarily commented out
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToList();
        }

        public IEnumerable<Product> GetByCategory(string category)
        {
            return _context.Products
                // .Include(p => p.Feedbacks) // Temporarily commented out
                .Where(p => p.Category == category)
                .ToList();
        }

        // Add
        public Product Add(Product product)
        {
            var entity = _context.Products.Add(product).Entity;
            _context.SaveChanges();
            return entity;
        }

        // Update
        public void Update(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        // Delete
        public void Delete(int id)
        {
            var product = GetById(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

        public void Delete(Product product)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        // Utility
        public bool Exists(int id)
        {
            return _context.Products.Any(p => p.ProductId == id);
        }

        public int Count()
        {
            return _context.Products.Count();
        }

        public int CountByTraderId(int traderId)
        {
            return _context.Products.Count(p => p.TraderId == traderId);
        }

        // Feedback stats - temporarily disabled due to Feedbacks table issues
        public double GetAverageRating(int productId)
        {
            // return _context.Feedbacks.Where(f => f.ProductId == productId).Average(f => f.Rating);
            return 0; // Temporarily return 0 until Feedbacks table is working
        }

        public int GetReviewCount(int productId)
        {
            // return _context.Feedbacks.Count(f => f.ProductId == productId);
            return 0; // Temporarily return 0 until Feedbacks table is working
        }
    }
}
