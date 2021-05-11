// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed record TagsFieldProperties : FieldProperties
    {
        public ImmutableList<string>? AllowedValues { get; init; }

        public LocalizedValue<ImmutableList<string>?> DefaultValues { get; init; }

        public ImmutableList<string>? DefaultValue { get; init; }

        public int? MinItems { get; init; }

        public int? MaxItems { get; init; }

        public TagsFieldEditor Editor { get; init; }

        public TagsFieldNormalization Normalization { get; init; }

        public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
        {
            return visitor.Visit((IField<TagsFieldProperties>)field, args);
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
