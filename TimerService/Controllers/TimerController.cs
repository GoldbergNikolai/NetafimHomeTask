using Microsoft.AspNetCore.Mvc;
using TimerService.Data;
using TimerService.DataModels;
using Timer = TimerService.DataModels.Timer;

namespace TimerService.Controllers
{
    [ApiController]
    [Route("api/timers")]
    public class TimerController : ControllerBase
    {
        private readonly TimerDbContext _context;

        public TimerController(TimerDbContext context)
        {
            _context = context;
        }

        // Endpoint to set a new timer
        [HttpPost]
        public async Task<IActionResult> SetTimer([FromBody] TimerRequest request)
        {
            var timer = new Timer
            {
                Id = Guid.NewGuid(),
                Hours = request.Hours,
                Minutes = request.Minutes,
                Seconds = request.Seconds,
                WebhookUrl = request.WebhookUrl,
                DateCreated = DateTime.UtcNow
            };

            _context.Timers.Add(timer);
            await _context.SaveChangesAsync();

            return Ok(new { id = timer.Id });
        }

        // Endpoint to get timer status
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimerStatus(Guid id)
        {
            var timer = await _context.Timers.FindAsync(id);
            if (timer == null)
            {
                return NotFound();
            }

            return Ok(new { id = timer.Id, timeLeft = timer.TimeLeft });
        }

        // Endpoint to list all timers with pagination
        [HttpGet]
        public IActionResult ListTimers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var timers = _context.Timers
                .OrderByDescending(t => t.DateCreated)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                pageNumber,
                pageSize,
                items = timers.Select(t => new
                {
                    t.Id,
                    t.DateCreated,
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    timeLeft = t.TimeLeft,
                    t.WebhookUrl,
                    t.Status
                }),
                totalRowCount = _context.Timers.Count()
            });
        }
    }

}
