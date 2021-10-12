// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed record ComponentsFieldProperties : FieldProperties
    {
        public int? MinItems { get; init; }

        public int? MaxItems { get; init; }

        public ImmutableList<string>? UniqueFields { get; init; }

        public DomainId SchemaId
        {
            init
            {
                if (value != default)
                {
                    SchemaIds = ImmutableList.Create(value);
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

        public ImmutableList<DomainId>? SchemaIds { get; init; }

        public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
        {
            return visitor.Visit((IField<ComponentsFieldProperties>)field, args);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
        {
            return Fields.Components(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
        {
            return Fields.Components(id, name, this, settings);
        }
    }
}
