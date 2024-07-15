using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using OhMyTLU.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace OhMyTLU.Services
{
    public class AccountService
    {
        private readonly OhMyTluContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public AccountService(OhMyTluContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }
        public async Task<bool> Login(LoginViewModel model)
        {
            var user = _context.Users.Where(u => u.Email == model.Email && u.Password == model.Password).FirstOrDefault();
            if (user != null)
            {
                var deviceId = GetDeviceId();
                UserSession us = new UserSession()
                {
                    UserId = user.Id,
                    StartTime = DateTime.Now,
                    DeviceId = deviceId,
                };
                _context.UserSessions.Add(us);
                await _context.SaveChangesAsync();
                var userSessionId = _context.UserSessions.Where(us => us.UserId == user.Id && us.EndTime == null).FirstOrDefault().Id;
                ActionAudit aa = new ActionAudit()
                {
                    UserSessionId = userSessionId,
                    Action = "Login",
                    Details = "",
                    Timestamp = DateTime.Now
                };
                _context.ActionAudits.Add(aa);
                await _context.SaveChangesAsync();
                _contextAccessor.HttpContext.Session.SetString("User", user.Name);
                _contextAccessor.HttpContext.Session.SetString("UserId", user.Id);
                _contextAccessor.HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                //_contextAccessor.HttpContext.Session.SetString("UserSessionId", _context.UserSessions.Where(us => us.UserId == user.Id && us.EndTime == null).FirstOrDefault().ToString());

                return true;
            }
            return false;
        }
        public async Task<bool> CheckAutoLogin(string deviceId)
        {
            var userSession = await _context.UserSessions
                .FirstOrDefaultAsync(us => us.DeviceId == deviceId && us.EndTime == null);

            if (userSession != null)
            {
                var user = await _context.Users.FindAsync(userSession.UserId);
                if (user != null)
                {
                    _contextAccessor.HttpContext.Session.SetString("User", user.Name);
                    _contextAccessor.HttpContext.Session.SetString("UserId", user.Id);
                    _contextAccessor.HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                    //_contextAccessor.HttpContext.Session.SetString("DeviceId", deviceId);
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> Register(RegisterViewModel model)
        {
            User user = new User()
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password
            };
            var existedUser = _context.Users.Where(u => u.Email == model.Email).FirstOrDefault();
            if (existedUser == null)
            {
                var deviceId = GetDeviceId();
                UserSession us = new UserSession()
                {
                    UserId = user.Id,
                    StartTime = DateTime.Now,
                    DeviceId = deviceId,
                };
                _context.UserSessions.Add(us);
                await _context.AddAsync(user);
                await _context.SaveChangesAsync();
                var userSessionId = _context.UserSessions.Where(us => us.UserId == user.Id && us.EndTime == null).FirstOrDefault().Id;
                ActionAudit aa = new ActionAudit()
                {
                    UserSessionId = userSessionId,
                    Action = "Register and Login",
                    Details = "",
                    Timestamp = DateTime.Now
                };
                _context.ActionAudits.Add(aa);
                await _context.SaveChangesAsync();
                _contextAccessor.HttpContext.Session.SetString("User", user.Name);
                _contextAccessor.HttpContext.Session.SetString("UserId", user.Id);

                return true;
            }
            return false;
        }
        public async Task Logout()
        {
            _contextAccessor.HttpContext.Session.Remove("User");

            _contextAccessor.HttpContext.Session.Remove("IsAdmin");
            var us = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault();
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;

            if (us != null)
            {
                us.EndTime = DateTime.Now;
                _context.SaveChanges();
            }
            
            _contextAccessor.HttpContext.Session.Remove("UserId");
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Logout",
                Details = "User logout app: ",
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            _contextAccessor.HttpContext.Response.Cookies.Delete("DeviceId");
        }
        private string GetDeviceId()
        {
            
            var deviceId = _contextAccessor.HttpContext.Request.Cookies["DeviceId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                _contextAccessor.HttpContext.Response.Cookies.Append("DeviceId", deviceId, cookieOptions);
            }
            return deviceId;
        }
    }
}
