using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using OhMyTLU.Hubs;
namespace OhMyTLU.Services
{
    public class MatchingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public MatchingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(async (state) =>
            {
                await MatchUsers();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(3)); 

            await Task.CompletedTask;
        }

        private async Task MatchUsers()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var hubContext = scope.ServiceProvider.GetRequiredService<RandomChatHub>();


                await hubContext.Match();

                Console.WriteLine("Matching users...");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(cancellationToken);
        }
    }
}
