using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Text.RegularExpressions;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Context;
using TaskManagement.Tests.Helpers;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Tests.Data
{
    [TestFixture]
    public class SeedDataTests
    {
        private ServiceProvider _serviceProvider;
        private TaskManagementDbContext _dbContext;
        private Mock<ILogger<TaskManagementDbContext>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            
            // Set up in-memory database with unique name to avoid conflicts
            var databaseName = $"SeedDataTest_{Guid.NewGuid()}";
            serviceCollection.AddDbContext<TaskManagementDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: databaseName));
            
            // Set up logger mock
            _loggerMock = new Mock<ILogger<TaskManagementDbContext>>();
            serviceCollection.AddSingleton(_loggerMock.Object);
            
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<TaskManagementDbContext>();
        }

        [TearDown]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _serviceProvider.Dispose();
        }

        [Test]
        public void Initialize_WhenDatabaseIsEmpty_SeedsDatabase()
        {
            // Arrange
            _dbContext.Database.EnsureDeleted();
            
            _loggerMock.SetupLog(LogLevel.Information, new Regex("Added 5 sample tasks"));
            
            // Act
            SeedData.Initialize(_serviceProvider);
            
            // Assert
            var tasks = _dbContext.Tasks.ToList();
            
            Assert.That(tasks, Is.Not.Null);
            Assert.That(tasks.Count, Is.EqualTo(5));
            
            Assert.That(tasks.Any(t => t.Name == "Implement user authentication"), Is.True);
            Assert.That(tasks.Any(t => t.Name == "Create API documentation"), Is.True);
            Assert.That(tasks.Any(t => t.Name == "Implement message queuing" && 
                                      t.AssignedTo == "john.doe@example.com"), Is.True);
            
            Assert.That(tasks.Any(t => t.Status == TaskStatus.NotStarted), Is.True);
            Assert.That(tasks.Any(t => t.Status == TaskStatus.InProgress), Is.True);
            Assert.That(tasks.Any(t => t.Status == TaskStatus.Completed), Is.True);
            
            _loggerMock.VerifyLog(LogLevel.Information, new Regex("Added 5 sample tasks"), Times.Once());
        }
        
        [Test]
        public void Initialize_WhenDatabaseHasData_DoesNotSeedDatabase()
        {
            // Arrange
            _dbContext.Tasks.Add(new TaskItem
            {
                Name = "Existing Task",
                Description = "This task already exists",
                Status = TaskStatus.NotStarted
            });
            _dbContext.SaveChanges();
            
            _loggerMock.SetupLog(LogLevel.Information, new Regex("Skipping task seeding"));
            
            // Act
            SeedData.Initialize(_serviceProvider);
            
            // Assert
            var tasks = _dbContext.Tasks.ToList();
            
            Assert.That(tasks, Is.Not.Null);
            Assert.That(tasks.Count, Is.EqualTo(1));
            Assert.That(tasks[0].Name, Is.EqualTo("Existing Task"));
            
            _loggerMock.VerifyLog(LogLevel.Information, new Regex("Skipping task seeding"), Times.Once());
        }
        
        [Test]
        public void Initialize_WhenExceptionOccurs_LogsErrorAndRethrows()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceScope = new Mock<IServiceScope>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockScopeServiceProvider = new Mock<IServiceProvider>();
            
            mockServiceScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);
            mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);
            
            var exception = new InvalidOperationException("Test exception");
            mockScopeServiceProvider.Setup(x => x.GetService(typeof(TaskManagementDbContext)))
                .Throws(exception);
            
            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(ILogger<TaskManagementDbContext>)))
                .Returns(_loggerMock.Object);
            
            _loggerMock.SetupLog(LogLevel.Error, exception, new Regex("An error occurred"));
            
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
                SeedData.Initialize(mockServiceProvider.Object));
            
            Assert.That(ex.Message, Is.EqualTo("Test exception"));
            
            _loggerMock.VerifyLog(LogLevel.Error, exception, new Regex("An error occurred"), Times.Once());
        }
    }
} 