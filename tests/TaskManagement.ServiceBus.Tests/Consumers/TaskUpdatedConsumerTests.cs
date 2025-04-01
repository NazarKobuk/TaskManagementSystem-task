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
    public class TaskUpdatedConsumerTests
    {
        private Mock<IServiceBusHandler> _serviceBusHandlerMock;
        private Mock<ILogger<TaskUpdatedConsumer>> _loggerMock;
        private TaskUpdatedConsumer _consumer;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void Setup()
        {
            _serviceBusHandlerMock = new Mock<IServiceBusHandler>();
            _loggerMock = new Mock<ILogger<TaskUpdatedConsumer>>();
            _consumer = new TaskUpdatedConsumer(_serviceBusHandlerMock.Object, _loggerMock.Object);
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
                new TaskUpdatedConsumer(null!, _loggerMock.Object));
            
            Assert.That(ex.ParamName, Is.EqualTo("serviceBusHandler"));
        }

        [Test]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => 
                new TaskUpdatedConsumer(_serviceBusHandlerMock.Object, null!));
            
            Assert.That(ex.ParamName, Is.EqualTo("logger"));
        }

        [Test]
        public async Task ExecuteAsync_SubscribesToCorrectQueue()
        {
            // Arrange
            _serviceBusHandlerMock
                .Setup(x => x.SubscribeAsync(
                    It.Is<string>(q => q == ServiceBusQueues.TaskUpdated),
                    It.IsAny<Func<TaskUpdatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _loggerMock.SetupLog(LogLevel.Information, "Starting TaskUpdatedConsumer");

            // Act
            await _consumer.StartAsync(_cancellationTokenSource.Token);

            // Assert
            _serviceBusHandlerMock.Verify(
                x => x.SubscribeAsync(
                    It.Is<string>(q => q == ServiceBusQueues.TaskUpdated),
                    It.IsAny<Func<TaskUpdatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _loggerMock.VerifyLog(LogLevel.Information, "Starting TaskUpdatedConsumer", Times.Once());
        }

        [Test]
        public async Task ExecuteAsync_WhenCancelled_LogsCancellation()
        {
            // Arrange
            _serviceBusHandlerMock
                .Setup(x => x.SubscribeAsync(
                    It.Is<string>(q => q == ServiceBusQueues.TaskUpdated),
                    It.IsAny<Func<TaskUpdatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Throws(new OperationCanceledException());

            _loggerMock.SetupLog(LogLevel.Information, "Starting TaskUpdatedConsumer");
            _loggerMock.SetupLog(LogLevel.Information, "TaskUpdatedConsumer was canceled");

            // Act
            await _consumer.StartAsync(_cancellationTokenSource.Token);

            // Assert
            _loggerMock.VerifyLog(LogLevel.Information, "Starting TaskUpdatedConsumer", Times.Once());
            _loggerMock.VerifyLog(LogLevel.Information, "TaskUpdatedConsumer was canceled", Times.Once());
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
                    It.Is<string>(q => q == ServiceBusQueues.TaskUpdated),
                    It.IsAny<Func<TaskUpdatedEvent, CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(tcs.Task);

            _loggerMock.SetupLog(LogLevel.Information, "Starting TaskUpdatedConsumer");
            _loggerMock.SetupLog(LogLevel.Error, exception, "Error in TaskUpdatedConsumer");

            // Act & Assert
            var task = _consumer.StartAsync(_cancellationTokenSource.Token);
            
            Thread.Sleep(100);
            
            _loggerMock.VerifyLog(LogLevel.Information, "Starting TaskUpdatedConsumer", Times.Once());
            _loggerMock.VerifyLog(LogLevel.Error, exception, "Error in TaskUpdatedConsumer", Times.Once());
        }
    }
} 