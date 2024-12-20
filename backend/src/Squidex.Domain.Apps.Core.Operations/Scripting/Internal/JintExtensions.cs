﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

public static class JintExtensions
{
    public static List<DomainId> ToIds(this JsValue? value)
    {
        var ids = new List<DomainId>();

        if (value is JsString s)
        {
            ids.Add(DomainId.Create(s.AsString()));
        }
        else if (value is JsArray a)
        {
            foreach (var item in a.OfType<JsString>())
            {
                ids.Add(DomainId.Create(item.AsString()));
            }
        }

        return ids;
    }

    internal static ScriptExecutionContext<T> ExtendWithAsyncFunctions<T>(this ScriptExecutionContext<T> context,
        IEnumerable<IJintExtension> extensions)
    {
        foreach (var extension in extensions)
        {
            extension.ExtendAsync(context);
        }

        return context;
    }

    internal static ScriptExecutionContext<T> ExtendWithFunctions<T>(this ScriptExecutionContext<T> context,
        IEnumerable<IJintExtension> extensions)
    {
        foreach (var extension in extensions)
        {
            extension.Extend(context);
        }

        return context;
    }

    internal static ScriptExecutionContext<T> ExtendWithVariables<T>(this ScriptExecutionContext<T> context,
        ScriptVars vars,
        ScriptOptions options)
    {
        var engine = context.Engine;

        context.CopyFrom(vars);

        if (options.AsContext)
        {
            var contextInstance = new WritableContext(engine, vars);

            engine.SetValue("ctx", contextInstance);
            engine.SetValue("context", contextInstance);
        }
        else
        {
            foreach (var (key, item) in vars)
            {
                engine.SetValue(key, item);
            }
        }

        engine.SetValue("async", true);

        return context;
    }
}
