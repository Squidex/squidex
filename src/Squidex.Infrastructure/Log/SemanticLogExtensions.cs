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
        public static void LogTrace(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log<None>(SemanticLogLevel.Trace, null, (_, w) => objectWriter(w));
        }

        public static void LogTrace<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Trace, context, objectWriter);
        }

        public static void LogDebug(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log<None>(SemanticLogLevel.Debug, null, (_, w) => objectWriter(w));
        }

        public static void LogDebug<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Debug, context, objectWriter);
        }

        public static void LogInformation(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log<None>(SemanticLogLevel.Information, null, (_, w) => objectWriter(w));
        }

        public static void LogInformation<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Information, context, objectWriter);
        }

        public static void LogWarning(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log<None>(SemanticLogLevel.Warning, null, (_, w) => objectWriter(w));
        }

        public static void LogWarning<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Warning, context, objectWriter);
        }

        public static void LogWarning(this ISemanticLog log, Exception exception, Action<IObjectWriter> objectWriter = null)
        {
            log.Log<None>(SemanticLogLevel.Warning, null, (_, w) => w.WriteException(exception, objectWriter));
        }

        public static void LogWarning<T>(this ISemanticLog log, Exception exception, T context, Action<T, IObjectWriter> objectWriter = null)
        {
            log.Log(SemanticLogLevel.Warning, context, (ctx, w) => w.WriteException(exception, ctx, objectWriter));
        }

        public static void LogError(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log<None>(SemanticLogLevel.Error, null, (_, w) => objectWriter(w));
        }

        public static void LogError<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Error, context, objectWriter);
        }

        public static void LogError(this ISemanticLog log, Exception exception, Action<IObjectWriter> objectWriter = null)
        {
            log.Log<None>(SemanticLogLevel.Error, null, (_, w) => w.WriteException(exception, objectWriter));
        }

        public static void LogError<T>(this ISemanticLog log, Exception exception, T context, Action<T, IObjectWriter> objectWriter = null)
        {
            log.Log(SemanticLogLevel.Error, context, (ctx, w) => w.WriteException(exception, ctx, objectWriter));
        }

        public static void LogFatal(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            log.Log<None>(SemanticLogLevel.Fatal, null, (_, w) => objectWriter(w));
        }

        public static void LogFatal<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            log.Log(SemanticLogLevel.Fatal, context, objectWriter);
        }

        public static void LogFatal(this ISemanticLog log, Exception exception, Action<IObjectWriter> objectWriter = null)
        {
            log.Log<None>(SemanticLogLevel.Fatal, null, (_, w) => w.WriteException(exception, objectWriter));
        }

        public static void LogFatal<T>(this ISemanticLog log, Exception exception, T context, Action<T, IObjectWriter> objectWriter = null)
        {
            log.Log(SemanticLogLevel.Fatal, context, (ctx, w) => w.WriteException(exception, ctx, objectWriter));
        }

        private static void WriteException(this IObjectWriter writer, Exception exception, Action<IObjectWriter> objectWriter)
        {
            objectWriter?.Invoke(writer);

            if (exception != null)
            {
                writer.WriteException(exception);
            }
        }

        private static void WriteException<T>(this IObjectWriter writer, Exception exception, T context, Action<T, IObjectWriter> objectWriter)
        {
            objectWriter?.Invoke(context, writer);

            if (exception != null)
            {
                writer.WriteException(exception);
            }
        }

        public static IObjectWriter WriteException(this IObjectWriter writer, Exception exception)
        {
            return writer.WriteObject(nameof(exception), exception, (ctx, w) =>
            {
                w.WriteProperty("type", ctx.GetType().FullName);
                w.WriteProperty("message", ctx.Message);
                w.WriteProperty("stackTrace", ctx.StackTrace);
            });
        }

        public static IDisposable MeasureTrace(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            return log.Measure<None>(SemanticLogLevel.Trace, null, (_, w) => objectWriter(w));
        }

        public static IDisposable MeasureTrace<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            return log.Measure(SemanticLogLevel.Trace, context, objectWriter);
        }

        public static IDisposable MeasureDebug(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            return log.Measure<None>(SemanticLogLevel.Debug, null, (_, w) => objectWriter(w));
        }

        public static IDisposable MeasureDebug<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            return log.Measure(SemanticLogLevel.Debug, context, objectWriter);
        }

        public static IDisposable MeasureInformation(this ISemanticLog log, Action<IObjectWriter> objectWriter)
        {
            return log.Measure<None>(SemanticLogLevel.Information, null, (_, w) => objectWriter(w));
        }

        public static IDisposable MeasureInformation<T>(this ISemanticLog log, T context, Action<T, IObjectWriter> objectWriter)
        {
            return log.Measure(SemanticLogLevel.Information, context, objectWriter);
        }

        private static IDisposable Measure<T>(this ISemanticLog log, SemanticLogLevel logLevel, T context, Action<T, IObjectWriter> objectWriter)
        {
            var watch = ValueStopwatch.StartNew();

            return new DelegateDisposable(() =>
            {
                var elapsedMs = watch.Stop();

                log.Log(logLevel, (Context: context, elapsedMs), (ctx, w) =>
                {
                    objectWriter?.Invoke(ctx.Context, w);

                    w.WriteProperty("elapsedMs", elapsedMs);
                });
            });
        }
    }
}
