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
    /// The situations this resolver actually answers, declared so the engine keeps the fast paths for
    /// everything else.
    /// </summary>
    /// <remarks>
    /// Deliberately omitted are <see cref="ReferenceResolverInterests.ObjectPropertyBase"/> and
    /// <see cref="ReferenceResolverInterests.PrimitivePropertyBase"/>, the pair that disables the
    /// non-computed member-read inline caches, the dense-array indexed-read lane and the member-call callee
    /// lane engine-wide. <see cref="TryPropertyReference"/> declines every base that is not null or
    /// undefined, so those are situations where the engine consulting this resolver could never change the
    /// result. Interests are a subscription filter and not a promise: a situation not subscribed to behaves
    /// exactly as if no resolver were registered.
    /// </remarks>
    public const ReferenceResolverInterests Interests =
        ReferenceResolverInterests.NullishPropertyBase |
        ReferenceResolverInterests.UnresolvableReference |
        ReferenceResolverInterests.NonCallableCallee;

    public static readonly NullPropagation Instance = new NullPropagation();

    /// <summary>
    /// Answers a read of a name that resolves to no binding, so that an unknown name does not throw a
    /// reference error.
    /// </summary>
    /// <remarks>
    /// Passing the reference base straight through hands script the engine's internal sentinel for the
    /// unresolvable state - a <see cref="JsString"/> reading <c>[[Unresolvable]]</c> - rather than
    /// <c>undefined</c>, which is documented on <see cref="IReferenceResolver.TryUnresolvableReference"/> and
    /// on <see cref="Reference.Base"/>. That is what scripts have always seen here, so it is kept and pinned
    /// by a test; assigning <see cref="JsValue.Undefined"/> instead would be the tidier behaviour but a
    /// breaking change for existing tenant scripts.
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
