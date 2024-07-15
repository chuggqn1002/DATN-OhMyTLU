using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using OhMyTLU.Helper;
using OhMyTLU.Models;
using OhMyTLU.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OhMyTLU.Hubs
{
    public class MessageHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public readonly static List<User> Users = new List<User>();
        private readonly static Dictionary<string, string> _ConnectionsMap = new Dictionary<string, string>();
        private readonly UserService userService;
        private readonly MessageService messageService;
        private readonly IHttpContextAccessor _contextAccessor;
        public MessageHub(UserService userService, IHttpContextAccessor contextAccessor, MessageService messageService)
        {
            this.userService = userService;
            _contextAccessor = contextAccessor;
            this.messageService = messageService;
        }

        public override Task OnConnectedAsync()
        {
            try
            {
                var user = userService.GetUsers().Where(u => u.Id == _contextAccessor.HttpContext.Session.GetString("UserId")).FirstOrDefault();
                if(user!=null)
                {
                    if (!Users.Any(u => u.Id == user.Id))
                    {
                        Users.Add(user);
                        _ConnectionsMap.Add(Context.ConnectionId, user.Id);
                    }

                    Clients.Others.SendAsync("joinMessage", user);
                }
                
            }
            catch (Exception ex)
            {
                Clients.Caller.SendAsync("onError", "OnConnected:" + ex.Message);
            }
            return base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var user = Users.Where(u => u.Id == _contextAccessor.HttpContext.Session.GetString("UserId")).FirstOrDefault();
                if(user!=null)
                    Users.Remove(user);

                // Tell other users to remove you from their list
                await Clients.All.SendAsync("leaveMessage", user);

                // Remove mapping
                _ConnectionsMap.Remove(Context.ConnectionId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("onError", "OnDisconnected: " + ex.Message);
            }

        }
        public List<User> GetUsers() { return Users; }
        public async Task SendMessage(string senderId,string senderName, string receiverId, string content)
        {
            var receiver = _ConnectionsMap.FirstOrDefault(x => x.Value == receiverId).Key;
            var message = await messageService.SendMessageAsync(senderId, receiverId, content);
            message.Content = BasicEmojis.ParseEmojis(message.Content);
            // Send the message
            if(receiver != null)
                await Clients.Client(receiver).SendAsync("newMessage", message);
            await Clients.Caller.SendAsync("newMessage", message);

        }

        public async Task Upload(Message message, string receiverId)
        {
            var receiver = _ConnectionsMap.FirstOrDefault(x => x.Value == receiverId).Key;
            // Send the message
            if (receiver != null)
                await Clients.Client(receiver).SendAsync("newMessage", message);
            await Clients.Caller.SendAsync("newMessage", message);
        }
        public async Task DeleteMessage(string messageId, string receiverId)
        {
            var receiver = _ConnectionsMap.FirstOrDefault(x => x.Value == receiverId).Key;
            if (messageId != null && receiver != null) { 
                await Clients.Client(receiver).SendAsync("deleteMessage", messageId);
            }
        }
        public async Task DeleteFriend(string senderId, string receiverId)
        {
            var receiver = _ConnectionsMap.FirstOrDefault(x => x.Value == receiverId).Key;
            if (senderId != null && receiver != null)
            {
                await Clients.Client(receiver).SendAsync("deleteFriend", senderId);
            }
        }
    }
}
