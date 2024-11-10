using TimerService.DataModels;

namespace TimerService.Services
{
    public interface ITimerLogicService
    {
        public Task<TimerResult> SetTimerAsync(TimerRequest request);
        public Task<TimerResult> GetTimerStatus(Guid id);
        public TimerListResult ListTimers(int pageNumber, int pageSize);
    }
}
