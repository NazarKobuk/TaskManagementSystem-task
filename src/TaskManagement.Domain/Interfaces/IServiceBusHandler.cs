using System.Threading;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Interfaces
{
    /// <summary>
    /// Interface for handling service bus operations
    /// </summary>
    public interface IServiceBusHandler
    {
        /// <summary>
        /// Sends a message to the service bus
        /// </summary>
        /// <typeparam name="T">Type of message to send</typeparam>
        /// <param name="message">Message to send</param>
        /// <param name="queueName">Name of the queue to send to</param>
        /// <param name="cancellationToken">Token for canceling the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SendMessageAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Subscribe to receive messages from the service bus
        /// </summary>
        /// <typeparam name="T">Type of message to receive</typeparam>
        /// <param name="queueName">Name of the queue to receive from</param>
        /// <param name="handler">Handler to process the received message</param>
        /// <param name="cancellationToken">Token for canceling the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SubscribeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class;
    }
} 