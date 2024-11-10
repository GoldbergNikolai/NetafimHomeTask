namespace TimerService.DataModels
{
    public class TimerListResult
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<TimerResult> Timers { get; set; }
        public int TotalRowCount { get; set; }

        public TimerListResult()
        {
            Timers = new List<TimerResult>();
        }
    }
}
