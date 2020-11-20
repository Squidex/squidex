// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class ReferencesFieldProperties : FieldProperties
    {
        public LocalizedValue<string[]?> DefaultValues { get; set; }

        public string[]? DefaultValue { get; set; }

        public int? MinItems { get; set; }

        public int? MaxItems { get; set; }

        public bool ResolveReference { get; set; }

        public bool AllowDuplicates { get; set; }

        public bool MustBePublished { get; set; }

        public ReferencesFieldEditor Editor { get; set; }

        public DomainId SchemaId
        {
            get
            {
                return SchemaIds?.FirstOrDefault() ?? default;
            }
            set
            {
                if (value != default)
                {
                    SchemaIds = new ReadOnlyCollection<DomainId>(new List<DomainId> { value });
                }
                else
                {
                    SchemaIds = null;
                }
            }
        }

        public ReadOnlyCollection<DomainId>? SchemaIds { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            return visitor.Visit((IField<ReferencesFieldProperties>)field);
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
}
