using TimerService.Data;

namespace TimerService.Services.BackgroundServices
{
    public class TimerExpirationService : BackgroundService
    {
        private readonly ILogger<TimerExpirationService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public TimerExpirationService(ILogger<TimerExpirationService> logger, 
                                      IServiceScopeFactory scopeFactory, 
                                      IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var client = _httpClientFactory.CreateClient();
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<TimerDbContext>();
                    var now = DateTime.UtcNow;

                    var expiredTimers = context.Timers
                        .Where(t => t.Status == Enums.Status.Started.ToString() && now >= t.DateCreated.AddHours(t.Hours).AddMinutes(t.Minutes).AddSeconds(t.Seconds))
                        .ToList();

                    foreach (var timer in expiredTimers)
                    {
                        try
                        {
                            timer.Status = Enums.Status.Finished.ToString();
                            await client.PostAsync(timer.WebhookUrl, null);

                            context.Timers.Update(timer);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(1000, stoppingToken); // Check every second
            }
        }
    }
}
