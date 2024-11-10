using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using TimerService.Data;
using TimerService.Profiles;
using TimerService.Services.BackgroundServices;
using Timer = TimerService.Data.Entities.Timer;
using Enums = TimerService.Enums;

namespace TimerServiceTests
{
    [TestFixture]
    public class TimerExpirationServiceTests
    {
        private Mock<ILogger<TimerExpirationService>> _logger;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpMessageHandler> _mockHandler;
        private TimerDbContext _dbContext;
        private IServiceScopeFactory _scopeFactory;
        private TimerExpirationService _timerExpirationService;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<TimerDbContext>()
                .UseInMemoryDatabase("TimerTestDb")
                .Options;
            _dbContext = new TimerDbContext(options);

            // Set up dependencies
            _logger = new Mock<ILogger<TimerExpirationService>>();
            _mockHandler = new Mock<HttpMessageHandler>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(_mockHandler.Object));

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();

            // Set up service scope
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(_dbContext);
            serviceCollection.AddSingleton(_logger.Object);
            _scopeFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

            // Initialize the TimerExpirationService
            _timerExpirationService = new TimerExpirationService(_logger.Object, _mapper, _scopeFactory, _mockHttpClientFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _timerExpirationService.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_ExpiredTimer_CallsWebhookAndUpdatesStatus()
        {
            // Arrange: Add an expired timer
            var timer = new Timer
            {
                Id = Guid.NewGuid(),
                Hours = 0,
                Minutes = 0,
                Seconds = 1,
                WebhookUrl = "https://example.com/webhook",
                DateCreated = DateTime.UtcNow.AddSeconds(-2),
                Status = Enums.Status.Started.ToString()
            };
            await _dbContext.Timers.AddAsync(timer);
            await _dbContext.SaveChangesAsync();

            // Set up mock to respond with success for the webhook call
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            await _timerExpirationService.StartAsync(CancellationToken.None);
            await Task.Delay(1500); // Wait for the background task to execute

            // Assert: Verify webhook was called and status updated
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString() == "https://example.com/webhook"),
                ItExpr.IsAny<CancellationToken>()
            );

            var updatedTimer = await _dbContext.Timers.FindAsync(timer.Id);
            Assert.AreEqual(Enums.Status.Finished.ToString(), updatedTimer.Status);
        }

        [Test]
        public async Task ExecuteAsync_FailedWebhookCall_LogsError()
        {
            // Arrange: Add an expired timer
            var timer = new Timer
            {
                Id = Guid.NewGuid(),
                Hours = 0,
                Minutes = 0,
                Seconds = 1,
                WebhookUrl = "https://example.com/webhook",
                DateCreated = DateTime.UtcNow.AddSeconds(-2),
                Status = Enums.Status.Started.ToString()
            };
            await _dbContext.Timers.AddAsync(timer);
            await _dbContext.SaveChangesAsync();

            // Set up mock to fail the webhook call
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Failed to reach webhook"));

            // Act
            await _timerExpirationService.StartAsync(CancellationToken.None);
            await Task.Delay(1500); // Wait for the background task to execute

            // Assert: Verify error was logged
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reach webhook")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_NonExpiredTimer_DoesNotCallWebhook()
        {
            // Arrange: Add a non-expired timer
            var timer = new Timer
            {
                Id = Guid.NewGuid(),
                Hours = 0,
                Minutes = 1,
                Seconds = 0,
                WebhookUrl = "https://example.com/webhook",
                DateCreated = DateTime.UtcNow,
                Status = Enums.Status.Started.ToString()
            };
            await _dbContext.Timers.AddAsync(timer);
            await _dbContext.SaveChangesAsync();

            // Act
            await _timerExpirationService.StartAsync(CancellationToken.None);
            await Task.Delay(1500); // Wait for the background task to execute

            // Assert: Verify webhook was not called
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
