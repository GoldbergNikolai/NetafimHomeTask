namespace TimerService.Data.Entities
{
    public class Timer
    {
        public Guid Id { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public string WebhookUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public string Status { get; set; } = Enums.Status.Started.ToString();
        public int TimeLeft => Math.Max(0, (int)(DateCreated.AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds) - DateTime.UtcNow).TotalSeconds);
    }
}
