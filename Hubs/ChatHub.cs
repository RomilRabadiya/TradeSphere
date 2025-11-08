using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TradeSphere3.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, bool> OnlineUsers = new ConcurrentDictionary<string, bool>();
        private static ConcurrentDictionary<string, int> UserToTraderMap = new ConcurrentDictionary<string, int>();

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;
            OnlineUsers.TryAdd(userId, true);
            
            await Clients.All.SendAsync("UserStatusChanged", userId, true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string userId = Context.UserIdentifier;
            OnlineUsers.TryRemove(userId, out _);
            UserToTraderMap.TryRemove(userId, out _);
            
            await Clients.All.SendAsync("UserStatusChanged", userId, false);
            await base.OnDisconnectedAsync(ex);
        }

        public async Task JoinTraderGroup(int traderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Trader_{traderId}");
            
            // Map user to trader ID
            string userId = Context.UserIdentifier;
            UserToTraderMap.AddOrUpdate(userId, traderId, (k, v) => traderId);
        }

        public async Task LeaveTraderGroup(int traderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Trader_{traderId}");
        }

        public async Task SendMessageToTrader(int receiverTraderId, object messageDto)
        {
            await Clients.Group($"Trader_{receiverTraderId}")
                .SendAsync("ReceiveMessage", messageDto);
            
            // Chat board update notification
            await Clients.All.SendAsync("UpdateChatBoard");
        }

        public async Task Typing(int receiverTraderId)
        {
            string userId = Context.UserIdentifier;
            int senderTraderId;
            UserToTraderMap.TryGetValue(userId, out senderTraderId);
            
            await Clients.Group($"Trader_{receiverTraderId}")
                .SendAsync("UserTyping", senderTraderId);
        }

        public static bool IsUserOnline(string userId)
        {
            return OnlineUsers.ContainsKey(userId);
        }
    }
}
