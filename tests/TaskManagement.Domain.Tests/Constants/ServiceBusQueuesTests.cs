using NUnit.Framework;
using TaskManagement.Domain.Constants;

namespace TaskManagement.Domain.Tests.Constants
{
    [TestFixture]
    public class ServiceBusQueuesTests
    {
        [Test]
        public void ServiceBusQueues_Constants_HaveExpectedValues()
        {
            // Assert
            Assert.That(ServiceBusQueues.TaskCreated, Is.EqualTo("task-created"));
            Assert.That(ServiceBusQueues.TaskUpdated, Is.EqualTo("task-updated"));
            Assert.That(ServiceBusQueues.TaskAssigned, Is.EqualTo("task-assigned"));
        }
    }
} 