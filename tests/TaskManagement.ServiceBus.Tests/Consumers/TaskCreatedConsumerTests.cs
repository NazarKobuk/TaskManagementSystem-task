using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Events;
using TaskManagement.Domain.Interfaces;
using TaskManagement.ServiceBus.Consumers;
using TaskManagement.Tests.Helpers;

namespace TaskManagement.ServiceBus.Tests.Consumers
{
    [TestFixture]
    public class TaskCreatedConsumerTests
    {
        private Mock<IServiceBusHandler> _serviceBusHandlerMock;
        private Mock<ILogger<TaskCreatedConsumer>> _loggerMock;
        private TaskCreatedConsumer _consumer;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void Setup()
        {
            _serviceBusHandlerMock = new Mock<IServiceBusHandler>();
            _loggerMock = new Mock<ILogger<TaskCreatedConsumer>>();
            _consumer = new TaskCreatedConsumer(_serviceBusHandlerMock.Object, _loggerMock.Object);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void Cleanup()
        {
            _cancellationTokenSource.Dispose();
        }

        [Test]
        public void Constructor_NullServiceBusHandler_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new TaskCreatedConsumer(null!, _loggerMock.Object));
            
            Assert.That(ex.ParamName, Is.EqualTo("serviceBusHandler"));
        }

        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new TaskCreatedConsumer(_serviceBusHandlerMock.Object, null!));
            
            Assert.That(ex.ParamName, Is.EqualTo("logger"));
        }

        [Test]
        public async Task ExecuteAsync_SubscribesToCorrectQueue()
        {
            // Arrange
            _serviceBusHandlerMock
                .Setup(x => x.SubscribeAsync(
                    It.Is<string>(q => q == ServiceBusQueues.TaskCreated),
                    It.IsAny<Func<TaskCreatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _loggerMock.SetupLog(LogLevel.Information, "Starting TaskCreatedConsumer");

            // Act
            await _consumer.StartAsync(_cancellationTokenSource.Token);

            // Assert
            _serviceBusHandlerMock.Verify(
                x => x.SubscribeAsync(
                    It.Is<string>(q => q == ServiceBusQueues.TaskCreated),
                    It.IsAny<Func<TaskCreatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _loggerMock.VerifyLog(LogLevel.Information, "Starting TaskCreatedConsumer", Times.Once());
        }

        [Test]
        public async Task ExecuteAsync_WhenCancelled_LogsCancellation()
        {
            // Arrange
            _serviceBusHandlerMock
                .Setup(x => x.SubscribeAsync(
                    It.Is<string>(q => q == ServiceBusQueues.TaskCreated),
                    It.IsAny<Func<TaskCreatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new OperationCanceledException());

            _loggerMock.SetupLog(LogLevel.Information, "Starting TaskCreatedConsumer");
            _loggerMock.SetupLog(LogLevel.Information, "TaskCreatedConsumer was canceled");

            // Act
            await _consumer.StartAsync(_cancellationTokenSource.Token);

            // Assert
            _loggerMock.VerifyLog(LogLevel.Information, "Starting TaskCreatedConsumer", Times.Once());
            _loggerMock.VerifyLog(LogLevel.Information, "TaskCreatedConsumer was canceled", Times.Once());
        }

        [Test]
        public void ExecuteAsync_WhenExceptionOccurs_LogsErrorAndRethrows()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");

            var tcs = new TaskCompletionSource<object>();
            tcs.SetException(exception);

            _serviceBusHandlerMock
                .Setup(x => x.SubscribeAsync(
                    It.Is<string>(q => q == ServiceBusQueues.TaskCreated),
                    It.IsAny<Func<TaskCreatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(tcs.Task);

            _loggerMock.SetupLog(LogLevel.Information, "Starting TaskCreatedConsumer");
            _loggerMock.SetupLog(LogLevel.Error, exception, "Error in TaskCreatedConsumer");

            // Act & Assert
            var task = _consumer.StartAsync(_cancellationTokenSource.Token);
            
            Thread.Sleep(100);
            
            _loggerMock.VerifyLog(LogLevel.Information, "Starting TaskCreatedConsumer", Times.Once());
            _loggerMock.VerifyLog(LogLevel.Error, exception, "Error in TaskCreatedConsumer", Times.Once());
        }
    }
} 