using System.Collections.Generic;
using System.Threading.Tasks;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public interface IProductRepository
    {
        // Get operations
        Product GetById(int id);
        Task<Product> GetByIdAsync(int id);
        IEnumerable<Product> GetAll();
        IEnumerable<Product> GetByTraderId(int traderId);
        IEnumerable<Product> GetByName(string name);
        IEnumerable<Product> GetByPriceRange(decimal minPrice, decimal maxPrice);
        IEnumerable<Product> GetByCategory(string category);

        // Add operations
        Product Add(Product product);

        // Update operations
        void Update(Product product);
        //Delete operations
        void Delete(int id);

        // Utility operations
        bool Exists(int id);
        int Count();
        int CountByTraderId(int traderId);

    }
}
