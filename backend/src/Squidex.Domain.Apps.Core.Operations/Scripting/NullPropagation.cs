// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class NullPropagation : IReferenceResolver
{
    public static readonly NullPropagation Instance = new NullPropagation();

    public bool TryUnresolvableReference(Engine engine, Reference reference, out JsValue value)
    {
        value = reference.Base;
        return true;
    }

    public bool TryGetCallable(Engine engine, object reference, out JsValue value)
    {
        value = new ClrFunction(engine, "anonymous", (thisObj, _) => thisObj);
        return true;
    }

    public bool TryPropertyReference(Engine engine, Reference reference, ref JsValue value)
    {
        return value.IsNull() || value.IsUndefined();
    }

    public bool CheckCoercible(JsValue value)
    {
        return true;
    }
}
