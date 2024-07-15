using Microsoft.EntityFrameworkCore;
using OhMyTLU.Data;
using OhMyTLU.Hubs;
using OhMyTLU.Middleware;
using OhMyTLU.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<OhMyTluContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddSingleton<RandomChatHub>();
builder.Services.AddHostedService<MatchingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//app.UseAuthorization();
app.UseMiddleware<AutoLoginMiddleware>();
app.MapHub<RandomChatHub>("/RandomChatHub");
app.MapHub<MessageHub>("/MessageHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
