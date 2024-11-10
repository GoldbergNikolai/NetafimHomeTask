using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TimerService.Data;
using TimerService.DataModels;
using TimerService.Profiles;
using TimerService.Services;

namespace TimerServiceTests
{
    [TestFixture]
    public class TimerServiceTests
    {
        private Mock<ILogger<TimerLogicService>> _logger;
        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private Mock<HttpMessageHandler> mockHandler;
        private TimerDbContext _dbContext;
        private TimerLogicService _timerLogicService;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<TimerDbContext>()
                .UseInMemoryDatabase("TimerTestDb")
                .Options;
            _dbContext = new TimerDbContext(options);

            _logger = new Mock<ILogger<TimerLogicService>>();

            var config = new MapperConfiguration(cfg => { cfg.AddProfile<MappingProfile>(); });
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();

            mockHandler = new Mock<HttpMessageHandler>();
            mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(mockHandler.Object));

            _timerLogicService = new TimerLogicService(_logger.Object, _mapper, _dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task SetTimer_ValidInput_ReturnsUniqueId()
        {
            var request = new TimerRequest { Hours = 0, Minutes = 1, Seconds = 0, WebhookUrl = "https://example.com/webhook" };

            var timer = await _timerLogicService.SetTimerAsync(request);

            Assert.IsNotNull(timer);
            Assert.IsFalse(string.IsNullOrEmpty(timer.Id.ToString()));
        }

        [Test]
        public async Task SetTimer_InvalidInput_ReturnsError()
        {
            var invalidRequest = new TimerRequest { Hours = -1, Minutes = -10, Seconds = 0, WebhookUrl = "" };

            var timer = await _timerLogicService.SetTimerAsync(invalidRequest);

            Assert.AreEqual(new Guid(), timer.Id);
        }

        [Test]
        public async Task GetTimerStatus_ActiveTimer_ReturnsTimeLeft()
        {
            var timer = await _timerLogicService.SetTimerAsync(new TimerRequest { Hours = 0, Minutes = 0, Seconds = 5, WebhookUrl = "https://example.com/webhook" });
            var status = await _timerLogicService.GetTimerStatus(timer.Id);

            Assert.GreaterOrEqual(status.TimeLeft, 0);
        }

        [Test]
        public async Task GetTimerStatus_ExpiredTimer_ReturnsZero()
        {
            var timer = await _timerLogicService.SetTimerAsync(new TimerRequest { Hours = 0, Minutes = 0, Seconds = 1, WebhookUrl = "https://example.com/webhook" });

            await Task.Delay(TimeSpan.FromSeconds(2)); // Allow timer to expire

            var status = await _timerLogicService.GetTimerStatus(timer.Id);

            Assert.AreEqual(0, status.TimeLeft);
        }

        [Test]
        public async Task SetTimer_PersistsToDatabase()
        {
            var timer = await _timerLogicService.SetTimerAsync(new TimerRequest { Hours = 0, Minutes = 1, Seconds = 0, WebhookUrl = "https://example.com/webhook" });

            var savedTimer = await _dbContext.Timers.FindAsync(timer.Id);

            Assert.IsNotNull(savedTimer);
            Assert.AreEqual("https://example.com/webhook", savedTimer.WebhookUrl);
        }

        [Test]
        public async Task TimerService_ReloadsTimersAfterRestart()
        {
            var timer = await _timerLogicService.SetTimerAsync(new TimerRequest { Hours = 0, Minutes = 0, Seconds = 5, WebhookUrl = "https://example.com/webhook" });

            // Simulate service restart
            var newTimerService = new TimerLogicService(_logger.Object, _mapper, _dbContext);

            var status = await newTimerService.GetTimerStatus(timer.Id);

            Assert.Greater(status.TimeLeft, 0); // Ensure timer is still running after restart
        }
    }
}