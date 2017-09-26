// ==========================================================================
//  SemanticLogExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Diagnostics;

namespace Squidex.Infrastructure.Log
{
    public static class SemanticLogExtensions
    {
        public static void LogTrace(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Trace, objectWriter);
        }

        public static IDisposable MeasureTrace(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            return new TimeMeasurer(log, SemanticLogLevel.Trace, objectWriter);
        }

        public static void LogDebug(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Debug, objectWriter);
        }

        public static IDisposable MeasureDebug(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            return new TimeMeasurer(log, SemanticLogLevel.Debug, objectWriter);
        }

        public static void LogInformation(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Information, objectWriter);
        }

        public static IDisposable MeasureInformation(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            return new TimeMeasurer(log, SemanticLogLevel.Information, objectWriter);
        }

        public static void LogWarning(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Warning, objectWriter);
        }

        public static void LogWarning(this ISemanticLog log, Exception exception, Action<IObjectWriter> objectWriter = null)
        {
            log.Log(SemanticLogLevel.Warning, writer => writer.WriteException(exception, objectWriter));
        }

        public static void LogError(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Error, objectWriter);
        }

        public static void LogError(this ISemanticLog log, Exception exception, Action<IObjectWriter> objectWriter = null)
        {
            log.Log(SemanticLogLevel.Error, writer => writer.WriteException(exception, objectWriter));
        }

        public static void LogFatal(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Fatal, objectWriter);
        }

        public static void LogFatal(this ISemanticLog log, Exception exception, Action<IObjectWriter> objectWriter = null)
        {
            log.Log(SemanticLogLevel.Fatal, writer => writer.WriteException(exception, objectWriter));
        }

        private static void WriteException(this IObjectWriter writer, Exception exception, Action<IObjectWriter> objectWriter)
        {
            objectWriter?.Invoke(writer);

            if (exception != null)
            {
                writer.WriteException(exception);
            }
        }

        public static IObjectWriter WriteException(this IObjectWriter writer, Exception exception)
        {
            return writer.WriteObject(nameof(exception), inner =>
            {
                inner.WriteProperty("type", exception.GetType().FullName);
                inner.WriteProperty("message", exception.Message);
                inner.WriteProperty("stackTrace", exception.StackTrace);
            });
        }

        private sealed class TimeMeasurer : IDisposable
        {
            private readonly Stopwatch watch = Stopwatch.StartNew();
            private readonly SemanticLogLevel logLevel;
            private readonly Action<IObjectWriter> objectWriter;
            private readonly ISemanticLog log;

            public TimeMeasurer(ISemanticLog log, SemanticLogLevel logLevel, Action<IObjectWriter> objectWriter)
            {
                this.logLevel = logLevel;
                this.log = log;

                this.objectWriter = objectWriter;
            }

            public void Dispose()
            {
                watch.Stop();

                log.Log(logLevel, writer =>
                {
                    objectWriter?.Invoke(writer);

                    writer.WriteProperty("elapsedMs", watch.ElapsedMilliseconds);
                });
            }
        }
    }
}
