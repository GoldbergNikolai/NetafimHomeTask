namespace TimerService.DataModels
{
    public class TimerRequest
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public string WebhookUrl { get; set; }
    }

}
