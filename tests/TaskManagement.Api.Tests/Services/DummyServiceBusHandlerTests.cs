using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManagement.Api;
using TaskManagement.Domain.Events;

namespace TaskManagement.Api.Tests.Services
{
    [TestFixture]
    public class DummyServiceBusHandlerTests
    {
        private Mock<ILogger<DummyServiceBusHandler>> _mockLogger = null!;
        private DummyServiceBusHandler _handler = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<DummyServiceBusHandler>>();
            _handler = new DummyServiceBusHandler(_mockLogger.Object);
        }

        [Test]
        public async Task SendMessageAsync_LogsWarning_ReturnsCompletedTask()
        {
            // Arrange
            var message = new TestMessage { Id = 1, Content = "Test" };
            var queueName = "test-queue";
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.SendMessageAsync(message, queueName, cancellationToken);

            // Assert
            // Verify warning was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("not sent") && 
                        v.ToString()!.Contains(queueName) && 
                        v.ToString()!.Contains(message.GetType().Name)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task SubscribeAsync_LogsWarning_ReturnsCompletedTask()
        {
            // Arrange
            var queueName = "test-queue";
            Func<TestMessage, CancellationToken, Task> handler = (msg, ct) => Task.CompletedTask;
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.SubscribeAsync<TestMessage>(queueName, handler, cancellationToken);

            // Assert
            // Verify warning was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("ignored") && 
                        v.ToString()!.Contains(queueName) && 
                        v.ToString()!.Contains(typeof(TestMessage).Name)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void Dispose_DoesNotThrowException()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _handler.Dispose());
        }

        // Test message class
        private class TestMessage
        {
            public int Id { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
} 