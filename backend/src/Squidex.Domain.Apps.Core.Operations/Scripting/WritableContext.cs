// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Native.Object;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting;

internal sealed class WritableContext : ObjectInstance
{
    private readonly ScriptVars vars;

    public WritableContext(Engine engine, ScriptVars vars)
        : base(engine)
    {
        this.vars = vars;

        foreach (var (key, value) in vars)
        {
            var property = key.ToCamelCase();

            if (value != null)
            {
                FastAddProperty(property, FromObject(engine, value), true, true, true);
            }
        }
    }

    public override bool Set(JsValue property, JsValue value, JsValue receiver)
    {
        var propertyName = property.AsString();

        vars[propertyName] = value.ToObject();

        return base.Set(property, value, receiver);
    }
}
