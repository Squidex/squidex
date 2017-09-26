// ==========================================================================
//  IScriptEngine.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public interface IScriptEngine
    {
        void Execute(ScriptContext context, string script);

        NamedContentData ExecuteAndTransform(ScriptContext context, string script);

        NamedContentData Transform(ScriptContext context, string script);
    }
}
