// ==========================================================================
//  SemanticLog.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Log
{
    public sealed class SemanticLog : ISemanticLog
    {
        private readonly IEnumerable<ILogChannel> channels;
        private readonly IEnumerable<ILogAppender> appenders;
        private readonly Func<IObjectWriter> writerFactory;

        public SemanticLog(
            IEnumerable<ILogChannel> channels,
            IEnumerable<ILogAppender> appenders,
            Func<IObjectWriter> writerFactory)
        {
            Guard.NotNull(channels, nameof(channels));
            Guard.NotNull(appenders, nameof(appenders));

            this.channels = channels;
            this.appenders = appenders;
            this.writerFactory = writerFactory;
        }

        public void Log(SemanticLogLevel logLevel, Action<IObjectWriter> action)
        {
            Guard.NotNull(action, nameof(action));

            var formattedText = FormatText(logLevel, action);

            List<Exception> exceptions = null;

            foreach (var channel in channels)
            {
                try
                {
                    channel.Log(logLevel, formattedText);
                }
                catch (Exception ex)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }

                    exceptions.Add(ex);
                }
            }

            if (exceptions != null && exceptions.Count > 0)
            {
                throw new AggregateException("An error occurred while writing to logger(s).", exceptions);
            }
        }

        private string FormatText(SemanticLogLevel logLevel, Action<IObjectWriter> objectWriter)
        {
            var writer = writerFactory();

            writer.WriteProperty(nameof(logLevel), logLevel.ToString());

            objectWriter(writer);

            foreach (var appender in appenders)
            {
                appender.Append(writer);
            }

            return writer.ToString();
        }

        public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
        {
            return new SemanticLog(channels, appenders.Union(new ILogAppender[] { new ConstantsLogWriter(objectWriter) }).ToArray(), writerFactory);
        }
    }
}
