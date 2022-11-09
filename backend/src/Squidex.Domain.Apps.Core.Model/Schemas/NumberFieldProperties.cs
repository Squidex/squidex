// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed record NumberFieldProperties : FieldProperties
{
    public ReadonlyList<double>? AllowedValues { get; init; }

    public LocalizedValue<double?> DefaultValues { get; init; }

    public double? DefaultValue { get; init; }

    public double? MaxValue { get; init; }

    public double? MinValue { get; init; }

    public bool IsUnique { get; init; }

    public bool InlineEditable { get; init; }

    public NumberFieldEditor Editor { get; init; }

    public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
    {
        return visitor.Visit(this, args);
    }

    public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
    {
        return visitor.Visit((IField<NumberFieldProperties>)field, args);
    }

    public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
    {
        return Fields.Number(id, name, partitioning, this, settings);
    }

    public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
    {
        return Fields.Number(id, name, this, settings);
    }
}
