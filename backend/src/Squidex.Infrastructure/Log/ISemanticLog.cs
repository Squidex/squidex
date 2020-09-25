// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public delegate void LogFormatter(IObjectWriter writer);

    public delegate void LogFormatter<in T>(T context, IObjectWriter writer);

    public interface ISemanticLog
    {
        void Log<T>(SemanticLogLevel logLevel, T context, Exception? exception, LogFormatter<T> action);

        void Log(SemanticLogLevel logLevel, Exception? exception, LogFormatter action);

        ISemanticLog CreateScope(Action<IObjectWriter> objectWriter);
    }
}
