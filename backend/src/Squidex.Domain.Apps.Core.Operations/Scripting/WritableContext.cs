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

        // Scripts touch a fraction of the variables, but mapping one is not always cheap: a content data
        // variable builds a wrapper, a user variable walks and groups every claim. The descriptors are
        // installed eagerly - so key order, enumeration and existence checks are exactly what they were -
        // and only the mapping waits for the first read of a value. Once it has run the descriptor drops
        // back to an ordinary data property and rejoins the write inline cache, which is what a
        // hand-written CustomJsValue descriptor cannot do.
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
