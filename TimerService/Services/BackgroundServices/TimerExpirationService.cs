﻿using AutoMapper;
using TimerService.Data;

namespace TimerService.Services.BackgroundServices
{
    public class TimerExpirationService : BackgroundService
    {
        private readonly ILogger<TimerExpirationService> _logger;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public TimerExpirationService(ILogger<TimerExpirationService> logger, 
                                      IMapper mapper,
                                      IServiceScopeFactory scopeFactory, 
                                      IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _mapper = mapper;
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
                        .Where(t => t.Status == Enums.Status.Started.ToString() && now >= t.DateCreated.AddHours(t.Hours).AddMinutes(t.Minutes).AddSeconds(t.Seconds))
                        .ToList();

                    foreach (var timer in expiredTimers)
                    {
                        try
                        {
                            timer.Status = Enums.Status.Finished.ToString();
                            using var client = _httpClientFactory.CreateClient();
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
