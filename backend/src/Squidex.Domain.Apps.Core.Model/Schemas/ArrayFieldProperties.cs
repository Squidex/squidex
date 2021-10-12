// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed record ArrayFieldProperties : FieldProperties
    {
        public int? MinItems { get; init; }

        public int? MaxItems { get; init; }

        public ImmutableList<string>? UniqueFields { get; init; }

        public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
        {
            return visitor.Visit((IArrayField)field, args);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
        {
            return Fields.Array(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
        {
            throw new NotSupportedException();
        }
    }
}
