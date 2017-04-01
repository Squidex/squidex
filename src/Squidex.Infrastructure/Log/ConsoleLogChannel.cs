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
            processor.EnqueueMessage(new LogMessageEntry { Message = message, Level = logLevel });
        }
    }
}
