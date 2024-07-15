using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using OhMyTLU.Helper;
using OhMyTLU.Hubs;
using OhMyTLU.Models;
using System;

namespace OhMyTLU.Services
{
    public class MessageService
    {
        private readonly OhMyTluContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public MessageService(IHttpContextAccessor contextAccessor, OhMyTluContext context)
        {
            _contextAccessor = contextAccessor;
            _context = context;
        }
        public async Task<Message> SendMessageAsync(string senderId, string receiverId, string content)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            await _context.Entry(message)
        .Reference(m => m.Sender)
        .LoadAsync();

            await _context.Entry(message)
                .Reference(m => m.Receiver)
                .LoadAsync();
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Send message",
                Details = "Send to " + receiverId +", content: "+ content,
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return message;

        }

        public async Task<List<MessageViewModel>> GetMessagesAsync(string senderId, string receiverId)
        {
            var messages = await (from message in _context.Messages
                                  join sender in _context.Users on message.SenderId equals sender.Id
                                  join receiver in _context.Users on message.ReceiverId equals receiver.Id
                                  where (message.SenderId == senderId && message.ReceiverId == receiverId) ||
                                        (message.SenderId == receiverId && message.ReceiverId == senderId)
                                  orderby message.Timestamp
                                  select new MessageViewModel
                                  {
                                      Id = message.Id,
                                      SenderId = message.SenderId,
                                      SenderName = sender.Name,
                                      ReceiverId = message.ReceiverId,
                                      ReceiverName = receiver.Name,
                                      Content = message.Content,
                                      Timestamp = message.Timestamp
                                  }).ToListAsync();
            foreach (var message in messages)
            {
                message.Content = BasicEmojis.ParseEmojis(message.Content);
            }
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Get messages",
                Details = "Get all message of " + receiverId + " And " + senderId,
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return messages;
        }

        public async Task<int> DeleteMessageAsync(string messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            int count;
            if (message != null)
            {
                _context.Messages.Remove(message);
                count = await _context.SaveChangesAsync();
                var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
                ActionAudit aa = new ActionAudit()
                {
                    UserSessionId = userSessionId,
                    Action = "Delete message",
                    Details = "Delete messageId: " + messageId,
                    Timestamp = DateTime.Now
                };
                _context.ActionAudits.Add(aa);
                await _context.SaveChangesAsync();
            }
            else count = 0;

            return count;
        }
        public async Task<List<MessageViewModel>> GetMessagesOf(string userId)
        {
            var messages = await (from message in _context.Messages
                                  join sender in _context.Users on message.SenderId equals sender.Id
                                  join receiver in _context.Users on message.ReceiverId equals receiver.Id
                                  where (message.SenderId == userId || message.ReceiverId == userId)
                                  orderby message.Timestamp
                                  select new MessageViewModel
                                  {
                                      Id = message.Id,
                                      SenderId = message.SenderId,
                                      SenderName = sender.Name,
                                      ReceiverId = message.ReceiverId,
                                      ReceiverName = receiver.Name,
                                      Content = message.Content,
                                      Timestamp = message.Timestamp
                                  }).ToListAsync();
            foreach (var message in messages)
            {
                message.Content = BasicEmojis.ParseEmojis(message.Content);
            }
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Get message",
                Details = "Get message of user: " + userId,
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return messages;
        }
    }
}
