// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public static class SemanticLogExtensions
    {
        public static void LogTrace(this ISemanticLog log, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Trace, null, action);
        }

        public static void LogTrace<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Trace, context, null, action);
        }

        public static void LogDebug(this ISemanticLog log, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Debug, null, action);
        }

        public static void LogDebug<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Debug, context, null, action);
        }

        public static void LogInformation(this ISemanticLog log, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Information, null, action);
        }

        public static void LogInformation<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Information, context, null, action);
        }

        public static void LogWarning(this ISemanticLog log, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Warning, null, action);
        }

        public static void LogWarning<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Warning, context, null, action);
        }

        public static void LogWarning(this ISemanticLog log, Exception exception, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Warning, exception, action);
        }

        public static void LogWarning<T>(this ISemanticLog log, Exception exception, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Warning, context, exception, action);
        }

        public static void LogError(this ISemanticLog log, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Error, null, action);
        }

        public static void LogError<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Error, context, null, action);
        }

        public static void LogError(this ISemanticLog log, Exception exception, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Error, exception, action);
        }

        public static void LogError<T>(this ISemanticLog log, Exception exception, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Error, context, exception, action);
        }

        public static void LogFatal(this ISemanticLog log, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Fatal, null, action);
        }

        public static void LogFatal<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Fatal, context, null, action);
        }

        public static void LogFatal(this ISemanticLog log, Exception? exception, LogFormatter action)
        {
            log.Log(SemanticLogLevel.Fatal, exception, action);
        }

        public static void LogFatal<T>(this ISemanticLog log, Exception? exception, T context, LogFormatter<T> action)
        {
            log.Log(SemanticLogLevel.Fatal, context, exception, action);
        }

        public static IObjectWriter WriteException(this IObjectWriter writer, Exception? exception)
        {
            if (exception == null)
            {
                return writer;
            }

            return writer.WriteObject(nameof(exception), exception, (ctx, w) =>
            {
                w.WriteProperty("type", ctx.GetType().FullName);

                if (ctx.Message != null)
                {
                    w.WriteProperty("message", ctx.Message);
                }

                if (ctx.StackTrace != null)
                {
                    w.WriteProperty("stackTrace", ctx.StackTrace);
                }
            });
        }

        public static IDisposable MeasureTrace(this ISemanticLog log, LogFormatter action)
        {
            return log.Measure(SemanticLogLevel.Trace, None.Value, (_, w) => action(w));
        }

        public static IDisposable MeasureTrace<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            return log.Measure(SemanticLogLevel.Trace, context, action);
        }

        public static IDisposable MeasureDebug(this ISemanticLog log, LogFormatter action)
        {
            return log.Measure(SemanticLogLevel.Debug, None.Value, (_, w) => action(w));
        }

        public static IDisposable MeasureDebug<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            return log.Measure(SemanticLogLevel.Debug, context, action);
        }

        public static IDisposable MeasureInformation(this ISemanticLog log, LogFormatter action)
        {
            return log.Measure(SemanticLogLevel.Information, None.Value, (_, w) => action(w));
        }

        public static IDisposable MeasureInformation<T>(this ISemanticLog log, T context, LogFormatter<T> action)
        {
            return log.Measure(SemanticLogLevel.Information, context, action);
        }

        private static IDisposable Measure<T>(this ISemanticLog log, SemanticLogLevel logLevel, T context, LogFormatter<T> action)
        {
            var watch = ValueStopwatch.StartNew();

            return new DelegateDisposable(() =>
            {
                var elapsedMs = watch.Stop();

                log.Log(logLevel, (Context: context, elapsedMs), null, (ctx, w) =>
                {
                    action?.Invoke(ctx.Context, w);

                    w.WriteProperty("elapsedMs", elapsedMs);
                });
            });
        }
    }
}
