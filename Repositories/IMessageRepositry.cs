using System;
using System.Collections.Generic;
using TradeSphere3.Models;

namespace TradeSphere3.Repositories
{
    public interface IMessageRepositry
    {

        Message GetById(int id);
        IEnumerable<Message> GetAll();
        Message Add(Message message);
        void Update(Message message);
        void Delete(int id);
        void Delete(Message message);
        bool Exists(int id);

        // Queries by sender/receiver
        IEnumerable<Message> GetBySenderId(string senderId);       // User sending
        IEnumerable<Message> GetByReceiverId(int receiverId); // Trader receiving

        // Queries by status
        IEnumerable<Message> GetUnreadByReceiver(int receiverId);
        IEnumerable<Message> GetReadByReceiver(int receiverId);

        // Queries by time
        IEnumerable<Message> GetRecentMessages(int receiverId, DateTime since);

        // Counts
        int Count();
        int CountUnreadByReceiver(int receiverId);
        int CountBySender(string senderId);

    }
}
