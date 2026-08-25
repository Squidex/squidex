// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting;

public interface IScript : IDisposable
{
    ContentData Transform(DataScriptVars vars);

    JsonValue Execute(ScriptVars vars);

    bool Evaluate(ScriptVars vars)
    {
        try
        {
            return Execute(vars).Equals(true);
        }
        catch
        {
            return false;
        }
    }
}
