// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed record UIFieldProperties : FieldProperties
{
    public UIFieldEditor Editor { get; init; }

    public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
    {
        return visitor.Visit(this, args);
    }

    public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
    {
        return visitor.Visit((IField<UIFieldProperties>)field, args);
    }

    public override RootField CreateRootField(long id, string name, Partitioning partitioning)
    {
        return Fields.UI(id, name, partitioning, this);
    }

    public override NestedField CreateNestedField(long id, string name)
    {
        return Fields.UI(id, name, this);
    }
}
