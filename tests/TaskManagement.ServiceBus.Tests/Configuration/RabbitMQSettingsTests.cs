using NUnit.Framework;
using TaskManagement.ServiceBus.Configuration;

namespace TaskManagement.ServiceBus.Tests.Configuration
{
    [TestFixture]
    public class RabbitMQSettingsTests
    {
        [Test]
        public void DefaultConstructor_SetsDefaultValues()
        {
            // Arrange & Act
            var settings = new RabbitMQSettings();
            
            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(settings.HostName, Is.EqualTo("localhost"));
                Assert.That(settings.Port, Is.EqualTo(5672));
                Assert.That(settings.UserName, Is.EqualTo("guest"));
                Assert.That(settings.Password, Is.EqualTo("guest"));
                Assert.That(settings.VirtualHost, Is.EqualTo("/"));
                Assert.That(settings.MaxRetryAttempts, Is.EqualTo(5));
                Assert.That(settings.RetryIntervalMs, Is.EqualTo(1000));
            });
        }
        
        [Test]
        public void Properties_WhenSet_ReturnExpectedValues()
        {
            // Arrange
            var settings = new RabbitMQSettings
            {
                HostName = "rabbit-server",
                Port = 15672,
                UserName = "admin",
                Password = "password123",
                VirtualHost = "/app",
                MaxRetryAttempts = 3,
                RetryIntervalMs = 2000
            };
            
            // Act & Assert
            Assert.Multiple(() =>
            {
                Assert.That(settings.HostName, Is.EqualTo("rabbit-server"));
                Assert.That(settings.Port, Is.EqualTo(15672));
                Assert.That(settings.UserName, Is.EqualTo("admin"));
                Assert.That(settings.Password, Is.EqualTo("password123"));
                Assert.That(settings.VirtualHost, Is.EqualTo("/app"));
                Assert.That(settings.MaxRetryAttempts, Is.EqualTo(3));
                Assert.That(settings.RetryIntervalMs, Is.EqualTo(2000));
            });
        }
    }
} 