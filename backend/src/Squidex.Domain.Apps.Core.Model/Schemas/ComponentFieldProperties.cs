// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed record ComponentFieldProperties : FieldProperties
{
    public DomainId SchemaId
    {
        init
        {
            SchemaIds = value != default ? ReadonlyList.Create(value) : null;
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
        return visitor.Visit((IField<ComponentFieldProperties>)field, args);
    }

    public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
    {
        return Fields.Component(id, name, partitioning, this, settings);
    }

    public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
    {
        return Fields.Component(id, name, this, settings);
    }
}
