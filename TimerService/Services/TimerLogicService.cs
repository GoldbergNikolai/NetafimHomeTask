using AutoMapper;
using TimerService.Data;
using TimerService.DataModels;
using Timer = TimerService.Data.Entities.Timer;

namespace TimerService.Services
{
    public class TimerLogicService : ITimerLogicService
    {
        private readonly ILogger<TimerLogicService> _logger;
        private readonly IMapper _mapper;
        private readonly TimerDbContext _dbContext;

        public TimerLogicService(ILogger<TimerLogicService> logger, IMapper mapper, TimerDbContext dbContext)
        {
            _logger = logger;
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<TimerResult> SetTimerAsync(TimerRequest request)
        {
            try
            {
                if (!IsInputValid(request)) return new TimerResult();

                var timer = _mapper.Map<Timer>(request);

                _dbContext.Timers.Add(timer);
                await _dbContext.SaveChangesAsync();

                return _mapper.Map<TimerResult>(timer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set timer by request: " +
                    $"WebhookUrl={request.WebhookUrl}, Hours={request.Hours}, Minutes={request.Minutes}, Seconds{request.Seconds}. " +
                    $"Message={ex.Message}");

                return new TimerResult();
            }
        }

        public async Task<TimerResult> GetTimerStatus(Guid id)
        {
            try
            {
                var timer = await _dbContext.Timers.FindAsync(id);

                return timer != null ? _mapper.Map<TimerResult>(timer) : new TimerResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get timer by id={id}. Message={ex.Message}");

                return new TimerResult();
            }
        }

        public TimerListResult ListTimers(int pageNumber, int pageSize)
        {
            try
            {
                var timers = _dbContext.Timers
                .OrderByDescending(t => t.DateCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

                return new TimerListResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRowCount = _dbContext.Timers.Count(),
                    Timers = timers.Select(t => _mapper.Map<TimerResult>(t)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to list timers. Message={ex.Message}");

                return new TimerListResult();
            }
        }

        private bool IsInputValid(TimerRequest request)
        {
            return request != null && 
                   request.Hours >= 0 && 
                   request.Minutes >= 0 && 
                   request.Seconds >= 0 &&
                   !string.IsNullOrWhiteSpace(request.WebhookUrl);
        }
    }
}
