// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
