// ==========================================================================
//  IScriptEngine.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public interface IScriptEngine
    {
        Task ExecuteAsync(ScriptContext context, string operationName, string script);

        Task<NamedContentData> ExecuteAndTransformAsync(ScriptContext context, string operationName, string script);
    }
}
