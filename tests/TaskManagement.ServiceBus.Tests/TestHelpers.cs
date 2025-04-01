using NUnit.Framework;

namespace TaskManagement.ServiceBus.Tests
{
    /// <summary>
    /// Helper methods for testing async code
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Asserts that the specified async function throws an exception of the expected type
        /// </summary>
        public static async Task<T> AssertThrowsAsync<T>(Func<Task> func) where T : Exception
        {
            try
            {
                await func();
                Assert.Fail($"Expected {typeof(T).Name} was not thrown");
                return null!;
            }
            catch (Exception ex)
            {
                if (ex is T expectedException)
                {
                    return expectedException;
                }

                // If we get an unexpected exception, rethrow it - don't swallow it
                throw;
            }
        }
    }
} 