// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas;

public class NestedField<T> : NestedField, IField<T> where T : FieldProperties, new()
{
    public T Properties { get; private set; }

    public override FieldProperties RawProperties
    {
        get => Properties;
    }

    public NestedField(long id, string name, T? properties = null, IFieldSettings? settings = null)
        : base(id, name, settings)
    {
        Properties = properties ?? new T();
    }

    [Pure]
    public override NestedField Update(FieldProperties newProperties)
    {
        var typedProperties = ValidateProperties(newProperties);

        if (Properties.Equals(typedProperties))
        {
            return this;
        }

        return Clone(clone =>
        {
            ((NestedField<T>)clone).Properties = typedProperties;
        });
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
