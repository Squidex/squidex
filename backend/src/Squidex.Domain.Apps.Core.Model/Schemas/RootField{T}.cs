// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas;

public record RootField<T> : RootField, IField<T> where T : FieldProperties, new()
{
    public T Properties { get; init; }

    public override FieldProperties RawProperties
    {
        get => Properties;
    }

    [Pure]
    public override RootField Update(FieldProperties newProperties)
    {
        var typedProperties = ValidateProperties(newProperties);

        if (Properties.Equals(typedProperties))
        {
            return this;
        }

        return this with { Properties = typedProperties };
    }

    private static T ValidateProperties(FieldProperties newProperties)
    {
        Guard.NotNull(newProperties);

        if (newProperties is not T typedProperties)
        {
            ThrowHelper.ArgumentException($"Properties must be of type '{typeof(T)}", nameof(newProperties));
            return default!;
        }

        return typedProperties;
    }

    public override TResult Accept<TResult, TArgs>(IFieldVisitor<TResult, TArgs> visitor, TArgs args)
    {
        return Properties.Accept(visitor, this, args);
    }
}
