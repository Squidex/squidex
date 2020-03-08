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
        Task ExecuteAsync(ScriptContext context, string script);

        Task<NamedContentData> ExecuteAndTransformAsync(ScriptContext context, string script);

        Task<NamedContentData> TransformAsync(ScriptContext context, string script);

        Task<IJsonValue> GetAsync(ScriptContext context, string script);

        bool Evaluate(ScriptContext context, string script);

        string? Interpolate(ScriptContext context, string script);
    }
}
