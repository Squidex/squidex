// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;

namespace Squidex.Domain.Apps.Core.Schemas;

public abstract record NestedField : INestedField
{
    public long Id { get; init; }

    public string Name { get; init; }

    public bool IsLocked { get; init; }

    public bool IsHidden { get; init; }

    public bool IsDisabled { get; init; }

    public abstract FieldProperties RawProperties { get; }

    [Pure]
    public NestedField Lock()
    {
        if (IsLocked)
        {
            return this;
        }

        return this with { IsLocked = true };
    }

    [Pure]
    public NestedField Hide()
    {
        if (IsHidden)
        {
            return this;
        }

        return this with { IsHidden = true };
    }

    [Pure]
    public NestedField Show()
    {
        if (!IsHidden)
        {
            return this;
        }

        return this with { IsHidden = false };
    }

    [Pure]
    public NestedField Disable()
    {
        if (IsDisabled)
        {
            return this;
        }

        return this with { IsDisabled = true };
    }

    [Pure]
    public NestedField Enable()
    {
        if (!IsDisabled)
        {
            return this;
        }

        return this with { IsDisabled = false };
    }

    public abstract T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, TArgs args);

    public abstract NestedField Update(FieldProperties newProperties);
}
