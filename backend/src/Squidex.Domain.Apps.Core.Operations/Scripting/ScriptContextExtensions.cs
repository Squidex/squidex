// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Native;
using Jint.Native.Object;

namespace Squidex.Domain.Apps.Core.Scripting
{
    internal static class ScriptContextExtensions
    {
        public static ExecutionContext AddContext(this ExecutionContext context, ScriptContext scriptContext)
        {
            var engine = context.Engine;

            var contextInstance = new ObjectInstance(engine);

            foreach (var (key, value) in scriptContext)
            {
                if (value != null)
                {
                    contextInstance.FastAddProperty(key, JsValue.FromObject(engine, value), true, true, true);
                    context[key] = value;
                }
            }

            engine.SetValue("ctx", contextInstance);
            engine.SetValue("context", contextInstance);

            return context;
        }
    }
}
