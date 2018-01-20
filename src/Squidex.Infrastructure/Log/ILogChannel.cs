// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Log
{
    public interface ILogChannel
    {
        void Log(SemanticLogLevel logLevel, string message);
    }
}