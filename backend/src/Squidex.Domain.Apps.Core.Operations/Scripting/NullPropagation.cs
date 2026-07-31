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
    /// <summary>
    /// The cases this resolver actually handles.
    /// </summary>
    /// <remarks>
    /// Without this list Jint has to assume that we want to see every property read and turns off its read
    /// caches for the whole engine. But <see cref="TryPropertyReference"/> only ever does something when the
    /// value is null or undefined, so the other cases can be left to Jint. Behavior does not change: for a
    /// case that is not listed here Jint behaves as if no resolver was registered at all.
    /// </remarks>
    public const ReferenceResolverInterests Interests =
        ReferenceResolverInterests.NullishPropertyBase |
        ReferenceResolverInterests.UnresolvableReference |
        ReferenceResolverInterests.NonCallableCallee;

    public static readonly NullPropagation Instance = new NullPropagation();

    /// <summary>
    /// Called when a name does not exist, so that reading an unknown variable does not throw.
    /// </summary>
    /// <remarks>
    /// The returned base is not <c>undefined</c> here but an internal Jint marker string that reads
    /// <c>[[Unresolvable]]</c>. That is what scripts have always seen, so it is kept as it is and covered by
    /// a test. Returning <c>undefined</c> would be nicer, but would change behavior for existing scripts.
    /// </remarks>
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
