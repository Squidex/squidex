// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public interface IScriptEngine
    {
        Task ExecuteAsync(ScriptVars vars, string script);

        Task<NamedContentData> ExecuteAndTransformAsync(ScriptVars vars, string script);

        Task<NamedContentData> TransformAsync(ScriptVars vars, string script);

        Task<IJsonValue> GetAsync(ScriptVars vars, string script);

        bool Evaluate(ScriptVars vars, string script);

        string? Interpolate(ScriptVars vars, string script);
    }
}
