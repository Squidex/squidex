// ==========================================================================
//  DebugLogChannel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Diagnostics;

namespace Squidex.Infrastructure.Log
{
    public sealed class DebugLogChannel : ILogChannel
    {
        public void Log(SemanticLogLevel logLevel, string message)
        {
            Debug.WriteLine(message);
        }
    }
}
