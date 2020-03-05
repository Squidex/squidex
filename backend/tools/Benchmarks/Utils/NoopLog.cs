// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Log;

namespace Benchmarks.Utils
{
    public sealed class NoopLog : ISemanticLog
    {
        public ISemanticLog CreateScope(Action<IObjectWriter> objectWriter)
        {
            return this;
        }

        public void Log<T>(SemanticLogLevel logLevel, T context, Exception exception, LogFormatter<T> action)
        {
        }

        public void Log(SemanticLogLevel logLevel, Exception exception, LogFormatter action)
        {
        }
    }
}
