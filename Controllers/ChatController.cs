using Microsoft.AspNetCore.Mvc;
using OhMyTLU.Data;

namespace OhMyTLU.Controllers
{
    public class ChatController : Controller
    {
        private readonly OhMyTluContext _DbContext;

        public ChatController(OhMyTluContext dbContext)
        {
            _DbContext = dbContext;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("User") == null) return Redirect("/Login");
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserName = HttpContext.Session.GetString("User");

            return View();
        }
        public IActionResult Message()
        {
            if (HttpContext.Session.GetString("User") == null) return Redirect("/Login");
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserName = HttpContext.Session.GetString("User");
            return View();
        }
        [HttpPost]
        public async Task<int> AddFriend([FromBody] AddFriendRequest request)
        {
            Console.WriteLine(request);
            string userId = HttpContext.Session.GetString("UserId");
            if (userId == null) { return 0; }
            Friend friend = new Friend()
            {
                UserId = userId,
                FriendId = request.otherUserId
            };
            var isFriend = _DbContext.Friends.Any(f => (f.UserId == friend.UserId && f.FriendId == friend.FriendId) || (f.UserId == friend.FriendId && f.FriendId == friend.UserId));
            if (isFriend) return 0;
            await _DbContext.Friends.AddAsync(friend);
            int result = await _DbContext.SaveChangesAsync();
            return result;
        }
        public class AddFriendRequest
        {
            public string otherUserId { get; set; }
        }
    }
}
