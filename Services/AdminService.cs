using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using X.PagedList;

namespace OhMyTLU.Services
{
    public class AdminService
    {
        private readonly OhMyTluContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        public AdminService(OhMyTluContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }
        public async Task<IPagedList<User>> GetUsers(int pageNumber, int pageSize)
        {
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Get",
                Details = "Admin view all users",
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return _context.Users.OrderBy(u => u.Name).ToPagedList(pageNumber, pageSize);
        }
        public async Task<User?> GetUser(string id)
        {
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Get",
                Details = "Admin view user with id: "+ _contextAccessor.HttpContext.Session.GetString("UserId"),
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
            return _context.Users.FirstOrDefault(u => u.Id == id); ;
        }
        public async Task<bool> CreateUser(User user)
        {
            var existedUser = _context.Users.Where(u => u.Email == user.Email).FirstOrDefault();
            if (existedUser == null)
            {
                await _context.AddAsync(user);
                await _context.SaveChangesAsync();
                var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
                ActionAudit aa = new ActionAudit()
                {
                    UserSessionId = userSessionId,
                    Action = "Add",
                    Details = "Admin Add user with id: " + user.Id,
                    Timestamp = DateTime.Now
                };
                _context.ActionAudits.Add(aa);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task EditUser(User user)
        {
            var u = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            u.Name = user.Name;
            u.Email = user.Email;
            u.Password = user.Password;
            u.Description = user.Description;
            await _context.SaveChangesAsync();
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Edit",
                Details = "Admin Edit user with id: " + user.Id,
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            var userSessionId = _context.UserSessions.Where(us => us.UserId == _contextAccessor.HttpContext.Session.GetString("UserId") && us.EndTime == null).FirstOrDefault().Id;
            ActionAudit aa = new ActionAudit()
            {
                UserSessionId = userSessionId,
                Action = "Delete",
                Details = "Admin Delete user with id: " + user.Id,
                Timestamp = DateTime.Now
            };
            _context.ActionAudits.Add(aa);
            await _context.SaveChangesAsync();
        }
    }
}
