using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Squidex.ICIS.Utilities
{
    public static class LogFactory<TMessage>
    {
        public static Action<IConsumer<string, TMessage>, string> ConsumerStats<T>(ILogger<T> logger)
        {
            return (producer, message) =>
            {
                Log(logger, message);
            };
        }

        public static Action<IConsumer<string, TMessage>, Error> ConsumerError<T>(ILogger<T> logger)
        {
            return (producer, message) =>
            {
                Log(logger, message);
            };
        }

        public static Action<IConsumer<string, TMessage>, LogMessage> ConsumerLog<T>(ILogger<T> logger)
        {
            return (producer, message) =>
            {
                Log(logger, message);
            };
        }

        public static Action<IProducer<string, TMessage>, string> ProducerStats<T>(ILogger<T> logger)
        {
            return (producer, message) =>
            {
                Log(logger, message);
            };
        }

        public static Action<IProducer<string, TMessage>, Error> ProducerError<T>(ILogger<T> logger)
        {
            return (producer, message) =>
            {
                Log(logger, message);
            };
        }

        public static Action<IProducer<string, TMessage>, LogMessage> ProducerLog<T>(ILogger<T> logger)
        {
            return (producer, message) =>
            {
                Log(logger, message);
            };
        }

        private static void Log<T>(ILogger<T> logger, string stats)
        {
            logger.LogError("Statistics: {stats}", stats);
        }

        private static void Log<T>(ILogger<T> logger, Error error)
        {
            logger.LogError("Error in kafka with code {code} and reason: {reason}", error.Code, error.Reason);
        }

        private static void Log<T>(ILogger<T> logger, LogMessage message)
        {
            switch (message.Level)
            {
                case SyslogLevel.Emergency:
                case SyslogLevel.Alert:
                case SyslogLevel.Critical:
                    if (logger.IsEnabled(LogLevel.Critical))
                    {
                        logger.LogCritical($"{{name}} {message.Message}", message.Name);
                    }

                    break;
                case SyslogLevel.Error:
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        logger.LogError($"{{name}} {message.Message}", message.Name);
                    }

                    break;
                case SyslogLevel.Warning:
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning($"{{name}} {message.Message}", message.Name);
                    }

                    break;
                case SyslogLevel.Notice:
                case SyslogLevel.Info:
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation($"{{name}} {message.Message}", message.Name);
                    }

                    break;
                case SyslogLevel.Debug:
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug($"{{name}} {message.Message}", message.Name);
                    }

                    break;
            }
        }
    }
}
