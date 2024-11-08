using TimerService.Data;

namespace TimerService.Services
{
    public class TimerExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public TimerExpirationService(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<TimerDbContext>();
                    var now = DateTime.UtcNow;

                    var expiredTimers = context.Timers
                        .Where(t => t.Status == "Started" && now >= t.DateCreated.AddHours(t.Hours).AddMinutes(t.Minutes).AddSeconds(t.Seconds))
                        .ToList();

                    foreach (var timer in expiredTimers)
                    {
                        timer.Status = "Finished";

                        using var client = _httpClientFactory.CreateClient();
                        await client.PostAsync(timer.WebhookUrl, null);

                        context.Timers.Update(timer);
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(1000, stoppingToken); // Check every second
            }
        }
    }
}
