// ==========================================================================
//  ISemanticLog.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public interface ISemanticLog
    {
        void Log(SemanticLogLevel logLevel, Action<IObjectWriter> action);

        ISemanticLog CreateScope(Action<IObjectWriter> objectWriter);
    }
}
