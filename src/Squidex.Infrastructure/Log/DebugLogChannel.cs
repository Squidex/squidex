// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
