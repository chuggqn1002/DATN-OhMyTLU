using Azure.Core.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OhMyTLU.Data;
using OhMyTLU.Models;
using OhMyTLU.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OhMyTLU.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OhMyTluContext _dbContext;
        private readonly AccountService accountService;

        public bool IsPasswordValid(string password)
        {
            // Yêu cầu ít nhất 8 ký tự, bao gồm ít nhất một chữ cái viết hoa, một chữ cái viết thường và một ký tự đặc biệt
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            return Regex.IsMatch(password, pattern);
        }
        public HomeController(ILogger<HomeController> logger, OhMyTluContext dbContext, AccountService accountService)
        {
            _logger = logger;
            _dbContext = dbContext;
            this.accountService = accountService;
        }

        public IActionResult Index()
        {
            Console.WriteLine(HttpContext.Session.Keys);
            return View();
        }
        [HttpPost("/Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!IsPasswordValid(model.Password))
            {
                ModelState.AddModelError("Password", "Password does not meet the requirements (Contain at least 1 upcase, lowcase, special character).");
                return View(model);
            }
            var result = await accountService.Login(model);
            // Tạo và ghi vào file
            string filePath = "wwwroot/check.txt";
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("userId session: "+HttpContext.Session.GetString("UserId"));
                    Console.Write("wwrite");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Có lỗi xảy ra: " + ex.Message);
            }
            if (result == true) return RedirectToAction("Index");
            ViewBag.error = "Login Failed: Youremail or password is incorrect";
            ModelState.AddModelError("", "Login Failed: Your user ID or password is incorrect");
            

           
            return View(model);
        }
        [Route("/Login")]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("User") != null) return RedirectToAction("index");
            return View();
        }
        [HttpPost("/Register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!IsPasswordValid(model.Password))
            {
                ModelState.AddModelError("Password", "Password does not meet the requirements (Contain at least 1 upcase, lowcase, special character).");
                return View(model);
            }

            var result = await accountService.Register(model);
            if (result == true) return RedirectToAction("Index");
            ModelState.AddModelError("", "Register Failed: Account is existed!");
            return View(model);
        }
        [Route("/Register")]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("User") != null) return RedirectToAction("index");
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await accountService.Logout();
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
