// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public interface IScriptEngine
    {
        Task<JsonValue2> ExecuteAsync(ScriptVars vars, string script, ScriptOptions options = default,
            CancellationToken ct = default);

        Task<ContentData> TransformAsync(DataScriptVars vars, string script, ScriptOptions options = default,
            CancellationToken ct = default);

        JsonValue2 Execute(ScriptVars vars, string script, ScriptOptions options = default);

        bool Evaluate(ScriptVars vars, string script, ScriptOptions options = default)
        {
            try
            {
                return Execute(vars, script, options).Equals(true);
            }
            catch
            {
                return false;
            }
        }
    }
}
