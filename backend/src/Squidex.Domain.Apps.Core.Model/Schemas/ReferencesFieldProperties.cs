// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed record ReferencesFieldProperties : FieldProperties
{
    public LocalizedValue<ReadonlyList<string>?> DefaultValues { get; init; }

    public ReadonlyList<string>? DefaultValue { get; init; }

    public int? MinItems { get; init; }

    public int? MaxItems { get; init; }

    public bool ResolveReference { get; init; }

    public bool AllowDuplicates { get; init; }

    public bool MustBePublished { get; init; }

    public ReferencesFieldEditor Editor { get; init; }

    public DomainId SchemaId
    {
        init
        {
            if (value != default)
            {
                SchemaIds = ReadonlyList.Create(value);
            }
            else
            {
                SchemaIds = null;
            }
        }
        get
        {
            return SchemaIds?.FirstOrDefault() ?? default;
        }
    }

    public ReadonlyList<DomainId>? SchemaIds { get; init; }

    public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
    {
        return visitor.Visit(this, args);
    }

    public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
    {
        return visitor.Visit((IField<ReferencesFieldProperties>)field, args);
    }

    public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
    {
        return Fields.References(id, name, partitioning, this, settings);
    }

    public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
    {
        return Fields.References(id, name, this, settings);
    }
}
