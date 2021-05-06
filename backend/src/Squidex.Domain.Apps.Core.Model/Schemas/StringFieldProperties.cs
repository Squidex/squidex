// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed record StringFieldProperties : FieldProperties
    {
        public ImmutableList<string>? AllowedValues { get; init; }

        public LocalizedValue<string?> DefaultValues { get; init; }

        public string? DefaultValue { get; init; }

        public string? Pattern { get; init; }

        public string? PatternMessage { get; init; }

        public string? FolderId { get; init; }

        public int? MinLength { get; init; }

        public int? MaxLength { get; init; }

        public int? MinCharacters { get; init; }

        public int? MaxCharacters { get; init; }

        public int? MinWords { get; init; }

        public int? MaxWords { get; init; }

        public bool IsUnique { get; init; }

        public bool InlineEditable { get; init; }

        public StringContentType ContentType { get; init; }

        public StringFieldEditor Editor { get; init; }

        public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
        {
            return visitor.Visit((IField<StringFieldProperties>)field, args);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
        {
            return Fields.String(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
        {
            return Fields.String(id, name, this, settings);
        }
    }
}
