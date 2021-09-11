// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public interface IScriptEngine
    {
        Task<IJsonValue> ExecuteAsync(ScriptVars vars, string script, ScriptOptions options = default,
            CancellationToken ct = default);

        Task<ContentData> TransformAsync(ScriptVars vars, string script, ScriptOptions options = default,
            CancellationToken ct = default);

        IJsonValue Execute(ScriptVars vars, string script, ScriptOptions options = default);

        bool Evaluate(ScriptVars vars, string script, ScriptOptions options = default)
        {
            try
            {
                return Execute(vars, script, options).Equals(JsonValue.True);
            }
            catch
            {
                return false;
            }
        }
    }
}
