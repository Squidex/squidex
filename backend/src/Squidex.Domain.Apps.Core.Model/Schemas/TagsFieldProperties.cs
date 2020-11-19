// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class TagsFieldProperties : FieldProperties
    {
        public ReadOnlyCollection<string>? AllowedValues { get; set; }

        public LocalizedValue<string[]?> DefaultValues { get; set; }

        public string[]? DefaultValue { get; set; }

        public int? MinItems { get; set; }

        public int? MaxItems { get; set; }

        public TagsFieldEditor Editor { get; set; }

        public TagsFieldNormalization Normalization { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            return visitor.Visit((IField<TagsFieldProperties>)field);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
        {
            return Fields.Tags(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
        {
            return Fields.Tags(id, name, this, settings);
        }
    }
}
