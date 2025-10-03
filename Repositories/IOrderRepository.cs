using System;
using System.Collections.Generic;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public interface IOrderRepository
    {
            // Get operations
            Order GetById(int id);
            IEnumerable<Order> GetAll();
        IEnumerable<Order> GetByUserId(string userId);
        IEnumerable<Order> GetByTraderId(int traderId);
        IEnumerable<Order> GetByProductId(int productId);
        IEnumerable<Order> GetByStatus(string status);
            IEnumerable<Order> GetByDateRange(DateTime startDate, DateTime endDate);

            // Add operations
            Order Add(Order order);

            // Update operations
            void Update(Order order);
            void UpdateStatus(int orderId, string status);
            void UpdateShippingAddress(int orderId, string newAddress);

            // Delete operations
            void Delete(int id);
            void Delete(Order order);

            // Utility operations
            bool Exists(int id);
            int Count();
            int CountByUserId(string userId);
            int CountByStatus(string status);

            // Business/analytics operations
            decimal GetTotalRevenue();
            decimal GetTotalRevenueByUserId(string userId);
            decimal GetTotalRevenueByDateRange(DateTime startDate, DateTime endDate);
    }
}
