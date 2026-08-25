// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting;

public interface IAsyncScript : IDisposable
{
    ValueTask<ContentData> TransformAsync(DataScriptVars vars,
        CancellationToken ct = default);

    ValueTask<JsonValue> ExecuteAsync(ScriptVars vars,
        CancellationToken ct = default);

    async ValueTask<bool> EvaluateAsync(ScriptVars vars,
        CancellationToken ct = default)
    {
        try
        {
            return (await ExecuteAsync(vars, ct)).Equals(true);
        }
        catch
        {
            return false;
        }
    }
}
