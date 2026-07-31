// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;

namespace Squidex.Domain.Apps.Core.Scripting;

internal sealed class WritableContext : ObjectInstance
{
    private readonly ScriptVars vars;

    public WritableContext(Engine engine, ScriptVars vars)
        : base(engine)
    {
        this.vars = vars;

        // Adds the value, but runs the conversion only when the script reads it for the first time. Most
        // scripts use a few of these variables and some of them are expensive, e.g. the user variable walks
        // and groups all claims. The properties themselves are added right away, so key order, enumeration
        // and "in" checks stay the same.
        foreach (var (key, item) in vars)
        {
            SetOwnProperty(key, PropertyDescriptor.CreateLazy(
                (Engine: engine, Item: item),
                static state => FromObject(state.Engine, state.Item)));
        }
    }

    public override bool Set(JsValue property, JsValue value, JsValue receiver)
    {
        var propertyName = property.AsString();

        vars.Set(propertyName, value.ToObject());

        return base.Set(property, value, receiver);
    }
}
