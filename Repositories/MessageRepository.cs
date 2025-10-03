using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeSphere3.Data;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public class MessageRepository : IMessageRepositry
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get by ID
        public Message GetById(int id)
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefault(m => m.MessageId == id);
        }

        // Get all messages
        public IEnumerable<Message> GetAll()
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .ToList();
        }

        // Add a message
        public Message Add(Message message)
        {
            var entity = _context.Messages.Add(message).Entity;
            _context.SaveChanges();
            return entity;
        }

        // Update a message
        public void Update(Message message)
        {
            _context.Messages.Update(message);
            _context.SaveChanges();
        }

        // Delete by ID
        public void Delete(int id)
        {
            var message = GetById(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
                _context.SaveChanges();
            }
        }

        // Delete by entity
        public void Delete(Message message)
        {
            _context.Messages.Remove(message);
            _context.SaveChanges();
        }

        // Check existence
        public bool Exists(int id)
        {
            return _context.Messages.Any(m => m.MessageId == id);
        }

        // Queries by sender (now using string UserId)
        public IEnumerable<Message> GetBySenderId(string senderId)
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == senderId)
                .ToList();
        }

        // Queries by receiver
        public IEnumerable<Message> GetByReceiverId(int  receiverId)
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.ReceiverId == receiverId)
                .ToList();
        }

        // Unread messages for receiver
        public IEnumerable<Message> GetUnreadByReceiver(int receiverId)
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == receiverId && m.Status == "Unread")
                .ToList();
        }

        // Read messages for receiver
        public IEnumerable<Message> GetReadByReceiver(int receiverId)
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == receiverId && m.Status == "Read")
                .ToList();
        }

        // Recent messages since a date
        public IEnumerable<Message> GetRecentMessages(int receiverId, DateTime since)
        {
            return _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == receiverId && m.SentDate >= since)
                .ToList();
        }

        // Total count
        public int Count()
        {
            return _context.Messages.Count();
        }

        // Count unread for receiver
        public int CountUnreadByReceiver(int receiverId)
        {
            return _context.Messages
                .Count(m => m.ReceiverId == receiverId && m.Status == "Unread");
        }

        // Count by sender (now using string UserId)
        public int CountBySender(string senderId)
        {
            return _context.Messages
                .Count(m => m.SenderId == senderId);
        }
    }
}
