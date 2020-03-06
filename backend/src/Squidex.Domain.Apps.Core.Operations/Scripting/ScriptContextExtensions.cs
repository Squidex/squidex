// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native.Object;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;

namespace Squidex.Domain.Apps.Core.Scripting
{
    internal static class ScriptContextExtensions
    {
        public static Engine AddContext(this Engine engine, ScriptContext context)
        {
            var contextInstance = new ObjectInstance(engine);

            if (context.Data != null)
            {
                contextInstance.FastAddProperty("data", new ContentDataObject(engine, context.Data), true, true, true);
            }

            if (context.DataOld != null)
            {
                contextInstance.FastAddProperty("oldData", new ContentDataObject(engine, context.DataOld), true, true, true);
            }

            if (context.User != null)
            {
                contextInstance.FastAddProperty("user", JintUser.Create(engine, context.User), false, true, false);
            }

            if (!string.IsNullOrWhiteSpace(context.Operation))
            {
                contextInstance.FastAddProperty("operation", context.Operation, false, false, false);
            }

            contextInstance.FastAddProperty("status", context.Status.ToString(), false, false, false);

            if (context.StatusOld != default)
            {
                contextInstance.FastAddProperty("oldStatus", context.StatusOld.ToString(), false, false, false);
            }

            engine.SetValue("ctx", contextInstance);
            engine.SetValue("context", contextInstance);

            return engine;
        }
    }
}
