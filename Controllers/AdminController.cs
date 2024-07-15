using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using OhMyTLU.Models;
using OhMyTLU.Services;
using System.Text.RegularExpressions;
using X.PagedList;

namespace OhMyTLU.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService adminService;

        public AdminController(AdminService adminService)
        {
            this.adminService = adminService;
        }

        public bool IsPasswordValid(string password)
        {
            // Yêu cầu ít nhất 8 ký tự, bao gồm ít nhất một chữ cái viết hoa, một chữ cái viết thường và một ký tự đặc biệt
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }
        public async Task<IActionResult> UserManage(int? page)
        {
            if (HttpContext.Session.GetString("IsAdmin") == null) return Redirect("/Login");
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var users = await adminService.GetUsers(pageNumber, pageSize);
            return View(users);
        }
        public IActionResult CreateUser()
        {
            if (HttpContext.Session.GetString("IsAdmin") == null) return Redirect("/Login");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (HttpContext.Session.GetString("IsAdmin") == null) return Redirect("/Login");
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            if (!IsPasswordValid(user.Password))
            {
                ModelState.AddModelError("Password", "Password does not meet the requirements (Contain at least 1 upcase, lowcase, special character).");
                return View(user);
            }

            var result = await adminService.CreateUser(user);
            if(result == true)
            {
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction("UserManage");
            }
            ModelState.AddModelError("", "Register Failed: Account is existed!");
            return View();
        }
        public async Task<IActionResult> EditUser(string id)
        {
            if (HttpContext.Session.GetString("IsAdmin") == null) return Redirect("/Login");
            var user = await adminService.GetUser(id);
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            if (HttpContext.Session.GetString("IsAdmin") == null) return Redirect("/Login");
            await adminService.EditUser(user);
            TempData["SuccessMessage"] = "User edited successfully.";
            return RedirectToAction("UserManage");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await adminService.GetUser(id);
            if (user == null)
            {
                return NotFound();
            }
            await adminService.DeleteUser(user);
            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction("UserManage");
        }
    }
}
