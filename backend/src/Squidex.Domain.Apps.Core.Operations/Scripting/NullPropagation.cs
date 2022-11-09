// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Jint.Runtime.References;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class NullPropagation : IReferenceResolver
{
    public static readonly NullPropagation Instance = new NullPropagation();

    public bool TryUnresolvableReference(Engine engine, Reference reference, out JsValue value)
    {
        value = reference.GetBase();

        return true;
    }

    public bool TryPropertyReference(Engine engine, Reference reference, ref JsValue value)
    {
        return value.IsNull() || value.IsUndefined();
    }

    public bool TryGetCallable(Engine engine, object reference, out JsValue value)
    {
        value = new ClrFunctionInstance(engine, "anonymous", (thisObj, _) => thisObj);

        return true;
    }

    public bool CheckCoercible(JsValue value)
    {
        return true;
    }
}
