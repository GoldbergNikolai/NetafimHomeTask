using Microsoft.AspNetCore.Mvc;
using TimerService.DataModels;
using TimerService.Services;

namespace TimerService.Controllers
{
    [ApiController]
    [Route("api/timers")]
    public class TimerController : ControllerBase
    {
        private readonly ITimerLogicService _timerLogicService;

        public TimerController(ITimerLogicService timerLogicService)
        {
            _timerLogicService = timerLogicService;
        }

        [HttpPost]
        public async Task<IActionResult> SetTimer([FromBody] TimerRequest request)
        {
            var result = await _timerLogicService.SetTimerAsync(request);
            
            return new JsonResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimerStatus(Guid id)
        {
            var result = await _timerLogicService.GetTimerStatus(id);
            
            return result.Id == new Guid() ? NotFound() : new JsonResult(result);
        }

        [HttpGet]
        public IActionResult ListTimers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var result = _timerLogicService.ListTimers(pageNumber, pageSize);

            return new JsonResult(result);
        }
    }
}
