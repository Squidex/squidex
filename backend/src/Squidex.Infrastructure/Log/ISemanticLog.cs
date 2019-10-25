// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public interface ISemanticLog
    {
        void Log<T>(SemanticLogLevel logLevel, T context, Action<T, IObjectWriter> action);

        ISemanticLog CreateScope(Action<IObjectWriter> objectWriter);
    }
}
