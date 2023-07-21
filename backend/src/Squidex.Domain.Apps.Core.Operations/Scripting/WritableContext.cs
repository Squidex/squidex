// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Squidex.Domain.Apps.Core.Scripting;

internal sealed class WritableContext : ObjectInstance
{
    private readonly ScriptVars vars;

    public WritableContext(Engine engine, ScriptVars vars)
        : base(engine)
    {
        this.vars = vars;

        foreach (var (key, item) in vars)
        {
            base.Set(key, FromObject(engine, item), this);
        }
    }

    public override bool Set(JsValue property, JsValue value, JsValue receiver)
    {
        var propertyName = property.AsString();

        vars.Set(propertyName, value.ToObject());

        return base.Set(property, value, receiver);
    }
}
