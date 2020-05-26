// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Log
{
    public sealed class SemanticLog : ISemanticLog
    {
        private readonly ILogChannel[] channels;
        private readonly ILogAppender[] appenders;
        private readonly IOptions<SemanticLogOptions> options;
        private readonly IObjectWriterFactory writerFactory;

        public SemanticLog(
            IOptions<SemanticLogOptions> options,
            IEnumerable<ILogChannel> channels,
            IEnumerable<ILogAppender> appenders,
            IObjectWriterFactory writerFactory)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(channels, nameof(channels));
            Guard.NotNull(appenders, nameof(appenders));
            Guard.NotNull(writerFactory, nameof(writerFactory));

            this.options = options;
            this.channels = channels.ToArray();
            this.appenders = appenders.ToArray();
            this.writerFactory = writerFactory;
        }

        public void Log<T>(SemanticLogLevel logLevel, T context, Exception? exception, LogFormatter<T> action)
        {
            Guard.NotNull(action, nameof(action));

            if (logLevel < options.Value.Level)
            {
                return;
            }

            var formattedText = FormatText(logLevel, context, exception, action);

            LogFormattedText(logLevel, formattedText);
        }

        public void Log(SemanticLogLevel logLevel, Exception? exception, LogFormatter action)
        {
            Guard.NotNull(action, nameof(action));

            if (logLevel < options.Value.Level)
            {
                return;
            }

            var formattedText = FormatText(logLevel, exception, action);

            LogFormattedText(logLevel, formattedText);
        }

        private void LogFormattedText(SemanticLogLevel logLevel, string formattedText)
        {
            List<Exception>? exceptions = null;

            for (var i = 0; i < channels.Length; i++)
            {
                try
                {
                    channels[i].Log(logLevel, formattedText);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            if (exceptions != null && exceptions.Count > 0)
            {
                throw new AggregateException("An error occurred while writing to logger(s).", exceptions);
            }
        }

        private string FormatText(SemanticLogLevel logLevel, Exception? exception, LogFormatter action)
        {
            var writer = writerFactory.Create();

            try
            {
                writer.WriteProperty(nameof(logLevel), logLevel.ToString());

                action(writer);

                for (var i = 0; i < appenders.Length; i++)
                {
                    appenders[i].Append(writer, logLevel, exception);
                }

                writer.WriteException(exception);

                return writer.ToString();
            }
            finally
            {
                writerFactory.Release(writer);
            }
        }

        private string FormatText<T>(SemanticLogLevel logLevel, T context, Exception? exception, LogFormatter<T> action)
        {
            var writer = writerFactory.Create();

            try
            {
                writer.WriteProperty(nameof(logLevel), logLevel.ToString());

                action(context, writer);

                for (var i = 0; i < appenders.Length; i++)
                {
                    appenders[i].Append(writer, logLevel, exception);
                }

                writer.WriteException(exception);

                return writer.ToString();
            }
            finally
            {
                writerFactory.Release(writer);
            }
        }

        public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
        {
            var newAppenders = appenders.Union(Enumerable.Repeat(new ConstantsLogWriter(objectWriter), 1));

            return new SemanticLog(options, channels, newAppenders, writerFactory);
        }
    }
}
