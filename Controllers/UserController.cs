using Microsoft.AspNetCore.Mvc;
using OhMyTLU.Data;
using OhMyTLU.Models;
using OhMyTLU.Services;
using System.Text.RegularExpressions;

namespace OhMyTLU.Controllers
{
    public class UserController : Controller
    {
        private readonly OhMyTluContext _Dbcontext;
        private readonly UserService userService;
        private readonly IWebHostEnvironment _environment;
        public UserController(OhMyTluContext context, UserService userService, IWebHostEnvironment environment)
        {
            _Dbcontext = context;
            this.userService = userService;
            _environment = environment;
        }
        public bool IsPasswordValid(string password)
        {
            // Yêu cầu ít nhất 8 ký tự, bao gồm ít nhất một chữ cái viết hoa, một chữ cái viết thường và một ký tự đặc biệt
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetString("User") == null) return Redirect("/Login");
            var userId = HttpContext.Session.GetString("UserId"); // Sử dụng cách phù hợp để lấy ID người dùng hiện tại
            var model = await userService.GetProfile(userId);
            ViewBag.UserId = userId;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!IsPasswordValid(model.Password))
            {
                ModelState.AddModelError("Password", "Password does not meet the requirements (Contain at least 1 upcase, lowcase, special character).");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId"); 
                
                await userService.UpdateProfile(userId, model);

                return RedirectToAction("Profile");
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult GetProfileImage(string userId)
        {
            var user = _Dbcontext.Users.Find(userId);
            if (user == null || user.Image == null)
            {
                string defaultImagePath = "/Icon/user.png";

                string physicalPath = _environment.WebRootPath + defaultImagePath;

                if (System.IO.File.Exists(physicalPath))
                {
                    return PhysicalFile(physicalPath, "image/png");
                }
                else
                {
                    return NotFound();
                }
            }

            return File(user.Image, "image/jpeg"); 
        }
    }
}
