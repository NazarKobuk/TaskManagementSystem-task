using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Moq;

namespace TaskManagement.Tests.Helpers
{
    public static class LoggerMockExtensions
    {
        public static Mock<ILogger<T>> SetupLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, string expectedMessage, Action? callback = null)
        {
            return logger.SetupLog(expectedLogLevel, (Exception?) null, expectedMessage, callback);
        }

        public static Mock<ILogger<T>> SetupLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Exception? expectedException, string expectedMessage, Action? callback = null)
        {
            Func<Exception?, bool> func = (exception) => exception == expectedException;
            
            return logger.SetupLog(expectedLogLevel, func, expectedMessage, callback);
        }
        public static Mock<ILogger<T>> SetupLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Func<Exception?, bool> expectedExceptionFunc, string expectedMessage, Action? callback = null)
        {
            Func<object, Type, bool> state = (v, t) => string.Equals(v.ToString(), expectedMessage, StringComparison.Ordinal);

            var setup = logger.Setup(mock => mock.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.Is<Exception>(exception => expectedExceptionFunc.Invoke(exception)),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            if (callback != null)
            {
                setup.Callback(callback);
            }

            return logger;
        }


        public static Mock<ILogger<T>> SetupLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Regex expectedMessageRegex, Action? callback = null)
        {
            return logger.SetupLog(expectedLogLevel, (Exception?)null, expectedMessageRegex, callback);
        }
        public static Mock<ILogger<T>> SetupLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Exception? expectedException, Regex expectedMessageRegex, Action? callback = null)
        {
            Func<Exception?, bool> func = (exception) => exception == expectedException;

            return logger.SetupLog(expectedLogLevel, func, expectedMessageRegex, callback);
        }
        public static Mock<ILogger<T>> SetupLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Func<Exception?, bool> expectedExceptionFunc, Regex expectedMessageRegex, Action? callback = null)
        {
            var setup = logger.Setup(mock => mock.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => expectedMessageRegex.IsMatch(v.ToString())),
                It.Is<Exception>(exception => expectedExceptionFunc.Invoke(exception)),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            if (callback != null)
            {
                setup.Callback(callback);
            }

            return logger;
        }

        public static Mock<ILogger<T>> VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, string expectedMessage, Times times)
        {
            return logger.VerifyLog(expectedLogLevel, (Exception?)null, expectedMessage, times);
        }

        public static Mock<ILogger<T>> VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Exception? expectedException, string expectedMessage, Times times)
        {
            Func<Exception?, bool> func = (exception) => exception == expectedException;
            return logger.VerifyLog(expectedLogLevel, func, expectedMessage, times);
        }
        public static Mock<ILogger<T>> VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Func<Exception?, bool> expectedExceptionFunc, string expectedMessage, Times times)
        {
            Func<object, Type, bool> state = (v, t) => string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0;
            logger.Verify(
                mock => mock.Log(
                    It.Is<LogLevel>(l => l == expectedLogLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.Is<Exception>(exception => expectedExceptionFunc.Invoke(exception)),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                times);

            return logger;
        }


        public static Mock<ILogger<T>> VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Regex expectedMessageRegex, Times times)
        {
            return logger.VerifyLog(expectedLogLevel, (Exception?)null, expectedMessageRegex, times);
        }
        public static Mock<ILogger<T>> VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Exception? expectedException, Regex expectedMessageRegex, Times times)
        {
            Func<Exception?, bool> func = (exception) => exception == expectedException;
            return logger.VerifyLog(expectedLogLevel, func, expectedMessageRegex, times);
        }
        public static Mock<ILogger<T>> VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel expectedLogLevel, Func<Exception?, bool> expectedExceptionFunc, Regex expectedMessageRegex, Times times)
        {
            logger.Verify(
                mock => mock.Log(
                    It.Is<LogLevel>(l => l == expectedLogLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => expectedMessageRegex.IsMatch(v.ToString())),
                    It.Is<Exception>(exception => expectedExceptionFunc.Invoke(exception)),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                times);

            return logger;
        }
    }
} 