using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using OhMyTLU.Data;
using OhMyTLU.Hubs;
using OhMyTLU.Models;
using System;

namespace OhMyTLU.Services
{
    public class UserService
    {
        private readonly OhMyTluContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(IHttpContextAccessor contextAccessor, OhMyTluContext context)
        {
            _contextAccessor = contextAccessor;
            _context = context;
        }
        public async Task<UserProfileViewModel> GetProfile(string id)
        {
            var user = _context.Users.Find(id);

            var model = new UserProfileViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Description = user.Description,
                ImageData = user.Image,
                Password = user.Password,
            };
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Get",
                Details = "User view profile: ",
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task UpdateProfile(string id, UserProfileViewModel model)
        {
            var user = _context.Users.Find(id);

            user.Name = model.Name;
            user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = model.Password; 
            }
            user.Description = model.Description;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.ImageFile.CopyToAsync(memoryStream);
                    user.Image = memoryStream.ToArray();
                }
            }

            await _context.SaveChangesAsync();
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Edit",
                Details = "User edit profile: ",
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
        }
        public List<User> GetUsers()
        {
            return _context.Users.ToList();
        }
        public User? GetUser(string id)
        {
            return _context.Users.Find(id);
        }
        public async Task<List<User>> GetFriends(string userId)
        {
            var friendIds = await _context.Friends
            .Where(f => f.UserId == userId)
            .Select(f => f.FriendId)
            .ToListAsync();

            var friendOfIds = await _context.Friends
                .Where(f => f.FriendId == userId)
                .Select(f => f.UserId)
                .ToListAsync();

            var allFriendIds = friendIds.Concat(friendOfIds).Distinct().ToList();

            var friends = await _context.Users
                .Where(u => allFriendIds.Contains(u.Id))
                .ToListAsync();
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Get friends",
                Details = "Get friend of user: " + userId,
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return friends;
        }
        public async Task<int> DeleteFriend(string userId, string friendId)
        {
            var friend =  _context.Friends
            .Where(f => (f.UserId == userId && f.FriendId == friendId) || (f.UserId == friendId && f.FriendId == userId)).FirstOrDefault();
             _context.Friends.Remove(friend);
            var count = await _context.SaveChangesAsync();
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Delete friend of" + userId,
                Details = "Delete friend " + friendId,
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return count;
        }
    }
}
