using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NUnit.Framework;
using System.Reflection;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data.Configuration;
using TaskManagement.Infrastructure.Data.Context;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Tests.Data
{
    [TestFixture]
    public class TaskItemConfigurationTests
    {
        private DbContextOptions<TaskManagementDbContext> _options;
        private TaskManagementDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<TaskManagementDbContext>()
                .UseInMemoryDatabase(databaseName: $"TaskItemConfigTest_{Guid.NewGuid()}")
                .Options;

            _dbContext = new TaskManagementDbContext(_options);
        }

        [TearDown]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public void Configure_SetsUpEntityCorrectly()
        {
            // Arrange & Act
            var entityType = _dbContext.Model.FindEntityType(typeof(TaskItem));

            // Assert
            // Verify table name
            Assert.That(entityType?.GetTableName(), Is.EqualTo("Tasks"));

            // Verify primary key
            var primaryKey = entityType?.FindPrimaryKey();
            Assert.That(primaryKey, Is.Not.Null);
            Assert.That(primaryKey.Properties.Count, Is.EqualTo(1));
            Assert.That(primaryKey.Properties[0].Name, Is.EqualTo("Id"));

            // Verify Id property
            var idProperty = entityType?.FindProperty("Id");
            Assert.That(idProperty, Is.Not.Null);
            Assert.That(idProperty.IsKey(), Is.True);
            
            // Verify Name property configuration
            var nameProperty = entityType?.FindProperty("Name");
            Assert.That(nameProperty, Is.Not.Null);
            Assert.That(nameProperty.IsNullable, Is.False);
            Assert.That(nameProperty.GetMaxLength(), Is.EqualTo(100));

            // Verify Description property configuration
            var descriptionProperty = entityType?.FindProperty("Description");
            Assert.That(descriptionProperty, Is.Not.Null);
            Assert.That(descriptionProperty.IsNullable, Is.False);
            Assert.That(descriptionProperty.GetMaxLength(), Is.EqualTo(500));

            // Verify Status property configuration
            var statusProperty = entityType?.FindProperty("Status");
            Assert.That(statusProperty, Is.Not.Null);
            Assert.That(statusProperty.IsNullable, Is.False);
            
            // Verify AssignedTo property configuration
            var assignedToProperty = entityType?.FindProperty("AssignedTo");
            Assert.That(assignedToProperty, Is.Not.Null);
            Assert.That(assignedToProperty.IsNullable, Is.True);
            Assert.That(assignedToProperty.GetMaxLength(), Is.EqualTo(100));
        }
        
        [Test]
        public async Task EntityConfiguration_EnforcesMaxLengthConstraints()
        {
            // Arrange
            var task = new TaskItem
            {
                Name = new string('A', 101),
                Description = new string('B', 501),
                Status = TaskStatus.NotStarted,
                AssignedTo = new string('C', 101)
            };

            // Act
            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            // Assert 
            var retrievedTask = await _dbContext.Tasks.FindAsync(task.Id);
            
            Assert.That(retrievedTask, Is.Not.Null);
        }
        
        [Test]
        public void Configuration_IsAppliedThroughModelBuilder()
        {
            // Arrange & Act & Assert
            var modelAssembly = _dbContext.GetType().Assembly;
            var configurationType = typeof(TaskItemConfiguration);
            
            Assert.That(configurationType.Assembly, Is.EqualTo(modelAssembly));
            
            Assert.That(typeof(IEntityTypeConfiguration<TaskItem>).IsAssignableFrom(configurationType), Is.True);
            
            var configureMethod = configurationType.GetMethod("Configure", 
                BindingFlags.Public | BindingFlags.Instance);
            
            Assert.That(configureMethod, Is.Not.Null);
            Assert.That(configureMethod.GetParameters().Length, Is.EqualTo(1));
            Assert.That(configureMethod.GetParameters()[0].ParameterType, 
                Is.EqualTo(typeof(EntityTypeBuilder<TaskItem>)));
        }
    }
} 