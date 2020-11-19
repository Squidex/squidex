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
    public sealed class NumberFieldProperties : FieldProperties
    {
        public ReadOnlyCollection<double>? AllowedValues { get; set; }

        public LocalizedValue<double?> DefaultValues { get; set; }

        public double? DefaultValue { get; set; }

        public double? MaxValue { get; set; }

        public double? MinValue { get; set; }

        public bool IsUnique { get; set; }

        public bool InlineEditable { get; set; }

        public NumberFieldEditor Editor { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            return visitor.Visit((IField<NumberFieldProperties>)field);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
        {
            return Fields.Number(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
        {
            return Fields.Number(id, name, this, settings);
        }
    }
}
