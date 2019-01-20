// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure.Log
{
    public sealed class SemanticLog : ISemanticLog
    {
        private readonly ILogChannel[] channels;
        private readonly ILogAppender[] appenders;
        private readonly IObjectWriterFactory writerFactory;

        public SemanticLog(
            IEnumerable<ILogChannel> channels,
            IEnumerable<ILogAppender> appenders,
            IObjectWriterFactory writerFactory)
        {
            Guard.NotNull(channels, nameof(channels));
            Guard.NotNull(appenders, nameof(appenders));
            Guard.NotNull(writerFactory, nameof(writerFactory));

            this.channels = channels.ToArray();
            this.appenders = appenders.ToArray();
            this.writerFactory = writerFactory;
        }

        public void Log<T>(SemanticLogLevel logLevel, T context, Action<T, IObjectWriter> action)
        {
            Guard.NotNull(action, nameof(action));

            var formattedText = FormatText(logLevel, context, action);

            LogFormattedText(logLevel, formattedText);
        }

        private void LogFormattedText(SemanticLogLevel logLevel, string formattedText)
        {
            List<Exception> exceptions = null;

            for (var i = 0; i < channels.Length; i++)
            {
                try
                {
                    channels[i].Log(logLevel, formattedText);
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

        private string FormatText<T>(SemanticLogLevel logLevel, T context, Action<T, IObjectWriter> objectWriter)
        {
            var writer = writerFactory.Create();

            try
            {
                writer.WriteProperty(nameof(logLevel), logLevel.ToString());

                objectWriter(context, writer);

                for (var i = 0; i < appenders.Length; i++)
                {
                    appenders[i].Append(writer);
                }

                return writer.ToString();
            }
            finally
            {
                writerFactory.Release(writer);
            }
        }

        public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
        {
            return new SemanticLog(channels, appenders.Union(new ILogAppender[] { new ConstantsLogWriter(objectWriter) }).ToArray(), writerFactory);
        }
    }
}
