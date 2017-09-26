// ==========================================================================
//  SemanticLogLogger.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.Extensions.Logging;

namespace Squidex.Infrastructure.Log.Adapter
{
    internal sealed class SemanticLogLogger : ILogger
    {
        private readonly ISemanticLog semanticLog;

        public SemanticLogLogger(ISemanticLog semanticLog)
        {
            this.semanticLog = semanticLog;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            SemanticLogLevel semanticLogLevel;

            switch (logLevel)
            {
                case LogLevel.Trace:
                    semanticLogLevel = SemanticLogLevel.Trace;
                    break;
                case LogLevel.Debug:
                    semanticLogLevel = SemanticLogLevel.Debug;
                    break;
                case LogLevel.Information:
                    semanticLogLevel = SemanticLogLevel.Information;
                    break;
                case LogLevel.Warning:
                    semanticLogLevel = SemanticLogLevel.Warning;
                    break;
                case LogLevel.Error:
                    semanticLogLevel = SemanticLogLevel.Error;
                    break;
                case LogLevel.Critical:
                    semanticLogLevel = SemanticLogLevel.Fatal;
                    break;
                default:
                    semanticLogLevel = SemanticLogLevel.Debug;
                    break;
            }

            semanticLog.Log(semanticLogLevel, writer =>
            {
                var message = formatter(state, exception);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    writer.WriteProperty(nameof(message), message);
                }

                if (eventId.Id > 0)
                {
                    writer.WriteObject(nameof(eventId), eventIdWriter =>
                    {
                        eventIdWriter.WriteProperty("id", eventId.Id);

                        if (!string.IsNullOrWhiteSpace(eventId.Name))
                        {
                            eventIdWriter.WriteProperty("name", eventId.Name);
                        }
                    });
                }

                if (exception != null)
                {
                    writer.WriteException(exception);
                }
            });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        private class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
