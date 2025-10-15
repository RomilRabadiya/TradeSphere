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
                .FirstOrDefault(p => p.ProductId == id);
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Trader)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public IEnumerable<Product> GetAll()
        {
            return _context.Products
                .Include(p => p.Trader)
                .ToList();
        }

        public IEnumerable<Product> GetByTraderId(int traderId)
        {
            return _context.Products
                .Where(p => p.TraderId == traderId)
                .ToList();
        }

        public IEnumerable<Product> GetByName(string name)
        {
            return _context.Products
                .Where(p => p.Name.Contains(name))
                .ToList();
        }

        public IEnumerable<Product> GetByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _context.Products
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



        public void Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                // Delete all feedback related to this product
                var feedbacks = _context.Feedbacks.Where(f => f.ProductId == id).ToList();
                if (feedbacks.Any())
                    _context.Feedbacks.RemoveRange(feedbacks);

                // Nullify product reference in Orders (keep them for records)
                var relatedOrders = _context.Orders.Where(o => o.ProductId == id).ToList();
                foreach (var order in relatedOrders)
                {
                    order.ProductId = null;
                    order.Product = null;
                }

                // Remove product itself
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

    }
}
