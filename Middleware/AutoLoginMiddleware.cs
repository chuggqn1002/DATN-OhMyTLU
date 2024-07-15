using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using OhMyTLU.Services;
using System;

namespace OhMyTLU.Middleware
{
    public class AutoLoginMiddleware
    {
        private readonly RequestDelegate _next;

        public AutoLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var scope = context.RequestServices.CreateScope())
            {
                //context.Session.Remove("User");
                //context.Session.Remove("IsAdmin");
                //context.Session.Remove("UserId");

                var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                var deviceId = context.Request.Cookies["DeviceId"];
                if (!string.IsNullOrEmpty(deviceId))
                {
                    var isAuthenticated = await accountService.CheckAutoLogin(deviceId);
                    if (isAuthenticated)
                    {
                        Console.WriteLine("Authen");
                    }
                }
            }

            await _next(context);
        }
    }

}
