// ==========================================================================
//  ConsoleLogChannel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Log.Internal;

namespace Squidex.Infrastructure.Log
{
    public sealed class ConsoleLogChannel : ILogChannel, IDisposable
    {
        private readonly ConsoleLogProcessor processor = new ConsoleLogProcessor();

        public void Dispose()
        {
            processor.Dispose();
        }

        public void Log(SemanticLogLevel logLevel, string message)
        {
            var color = 0;

            if (logLevel == SemanticLogLevel.Warning)
            {
                color = 0xffff00;
            }
            else if (logLevel >= SemanticLogLevel.Error)
            {
                color = 0xff0000;
            }

            processor.EnqueueMessage(new LogMessageEntry { Message = message, Color = color });
        }
    }
}
