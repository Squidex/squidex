// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;

namespace Squidex.Domain.Apps.Core.Scripting;

public interface IJintExtension
{
    void Extend(Engine engine)
    {
    }

    void Extend(ScriptExecutionContext context)
    {
    }

    void ExtendAsync(ScriptExecutionContext context)
    {
    }
}
