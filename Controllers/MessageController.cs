using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using OhMyTLU.Hubs;
using OhMyTLU.Models;
using OhMyTLU.Services;
using System.Text.RegularExpressions;

namespace OhMyTLU.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly UserService userService;
        private readonly MessageService messageService;
        private readonly IWebHostEnvironment _environment;

        public MessageController(IHubContext<MessageHub> hubContext, UserService userService, MessageService messageService, IWebHostEnvironment environment)
        {
            _hubContext = hubContext;
            this.userService = userService;
            this.messageService = messageService;
            _environment = environment;
        }
        [HttpGet("GetFriends")]
        public async Task<ActionResult> GetFriends(string id)
        {
            var friends = await userService.GetFriends(id);
            if (friends == null || friends.Count == 0)
                return NotFound();
            return Ok(friends);
        }
        [HttpPost("DeleteFriend")]
        public async Task<ActionResult> DeleteFriend(string userId, string friendId)
        {
            var result = await userService.DeleteFriend(userId, friendId);
            if (result == 0)
                return BadRequest();
            return Ok(result);
        }
        [HttpGet("GetMessages")]
        public async Task<ActionResult> GetMessages(string senderId, string receiverId)
        {
            var messages = await messageService.GetMessagesAsync(senderId, receiverId);
            return Ok(messages);
        }
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile File, [FromForm] string senderId, [FromForm] string receiverId)
        {
            //if (ModelState.IsValid)
            //{
            //    if (!_fileValidator.IsValid(viewModel.File))
            //        return BadRequest("Validation failed!");
            if (File == null || File.Length == 0)
                return BadRequest("No file uploaded.");
            var fileName = DateTime.UtcNow.ToString("yyyymmddMMss") + "_" + File.FileName;
                var folderPath = Path.Combine(_environment.WebRootPath, "uploads");
                var filePath = Path.Combine(folderPath, fileName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await File.CopyToAsync(fileStream);
                }

                string htmlImage = string.Format(
                    "<a href=\"/uploads/{0}\" target=\"_blank\">" +
                    "<img src=\"/uploads/{0}\" class=\"post-image\">" +
                    "</a>", fileName);

                var message = await messageService.SendMessageAsync(senderId, receiverId, Regex.Replace(htmlImage, @"(?i)<(?!img|a|/a|/img).*?>", string.Empty));

                return Ok(message);
            //}

            //return BadRequest();
        }
        [HttpPost("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var result = await messageService.DeleteMessageAsync(id);
            if(result == 0) return BadRequest(result);
            return Ok(result);
        }
        [HttpGet("GetMessagesOf")]
        public async Task<ActionResult> GetMessagesOf(string userId)
        {
            var messages = await messageService.GetMessagesOf(userId);
            return Ok(messages);
        }
    }
}
