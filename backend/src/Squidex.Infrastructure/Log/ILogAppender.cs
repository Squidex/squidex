// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Log
{
    public interface ILogAppender
    {
        void Append(IObjectWriter writer, SemanticLogLevel logLevel, Exception? exception);
    }
}
