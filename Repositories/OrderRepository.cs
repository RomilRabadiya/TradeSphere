using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeSphere3.Data;
using TradeSphere3.Models;
using System.Threading.Tasks;

namespace TradeSphere3.Repositories
{
    public class OrderRepository : IOrderRepository
    {

        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get operations
        public Order GetById(int id)
        {
            return _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderId == id);
        }

        public IEnumerable<Order> GetAll()
        {
            return _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .ToList();
        }

        public IEnumerable<Order> GetByUserId(string userId)
        {
            return _context.Orders
                .Include(o => o.Product)
                .Where(o => o.UserId == userId)
                .ToList();
        }

        public IEnumerable<Order> GetByTraderId(int traderId)
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .Where(o => o.TraderId == traderId) // ✅ now safe and always valid
                .ToList();
        }

        public IEnumerable<Order> GetByProductId(int productId)
        {
            return _context.Orders
                .Include(o => o.User)
                .Where(o => o.ProductId == productId)
                .ToList();
        }

        public IEnumerable<Order> GetByStatus(string status)
        {
            return _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Where(o => o.Status == status)
                .ToList();
        }

        public IEnumerable<Order> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Orders
                .Include(o => o.Product)
                .Include(o => o.User)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToList();
        }

        // Add
        public Order Add(Order order)
        {
            var entity = _context.Orders.Add(order).Entity;
            _context.SaveChanges();
            return entity;
        }

        // Update
        public void Update(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        public void UpdateStatus(int orderId, string status)
        {
            var order = GetById(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
            }
        }

        public void UpdateShippingAddress(int orderId, string newAddress)
        {
            var order = GetById(orderId);
            if (order != null)
            {
                order.ShippingAddress = newAddress;
                _context.SaveChanges();
            }
        }

        // Delete
        public void Delete(int id)
        {
            var order = GetById(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }
        }

        public void Delete(Order order)
        {
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }

        // Utility
        public bool Exists(int id)
        {
            return _context.Orders.Any(o => o.OrderId == id);
        }

        public int Count()
        {
            return _context.Orders.Count();
        }

        public int CountByUserId(string userId)
        {
            return _context.Orders.Count(o => o.UserId == userId);
        }

        public int CountByStatus(string status)
        {
            return _context.Orders.Count(o => o.Status == status);
        }

        // Analytics
        public decimal GetTotalRevenue()
        {
            return _context.Orders.Sum(o => o.TotalAmount);
        }

        public decimal GetTotalRevenueByUserId(string userId)
        {
            return _context.Orders
                .Where(o => o.UserId == userId)
                .Sum(o => o.TotalAmount);
        }

        public decimal GetTotalRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Sum(o => o.TotalAmount);
        }


        public async Task<bool> HasOrdersForProductAsync(int productId)
        {
            return await _context.Orders.AnyAsync(oi => oi.ProductId == productId);
        }
    }
}
