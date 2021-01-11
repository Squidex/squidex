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
    public sealed class StringFieldProperties : FieldProperties
    {
        public ReadOnlyCollection<string>? AllowedValues { get; set; }

        public LocalizedValue<string?> DefaultValues { get; set; }

        public string? DefaultValue { get; set; }

        public string? Pattern { get; set; }

        public string? PatternMessage { get; set; }

        public int? MinLength { get; set; }

        public int? MaxLength { get; set; }

        public int? MinCharacters { get; set; }

        public int? MaxCharacters { get; set; }

        public int? MinWords { get; set; }

        public int? MaxWords { get; set; }

        public bool IsUnique { get; set; }

        public bool InlineEditable { get; set; }

        public StringContentType ContentType { get; set; }

        public StringFieldEditor Editor { get; set; }

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
