// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class DateTimeFieldProperties : FieldProperties
    {
        public LocalizedValue<Instant?> DefaultValues { get; set; }

        public Instant? DefaultValue { get; set; }

        public Instant? MaxValue { get; set; }

        public Instant? MinValue { get; set; }

        public DateTimeCalculatedDefaultValue? CalculatedDefaultValue { get; set; }

        public DateTimeFieldEditor Editor { get; set; }

        public override T Accept<T, TArgs>(IFieldPropertiesVisitor<T, TArgs> visitor, TArgs args)
        {
            return visitor.Visit(this, args);
        }

        public override T Accept<T, TArgs>(IFieldVisitor<T, TArgs> visitor, IField field, TArgs args)
        {
            return visitor.Visit((IField<DateTimeFieldProperties>)field, args);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning, IFieldSettings? settings = null)
        {
            return Fields.DateTime(id, name, partitioning, this, settings);
        }

        public override NestedField CreateNestedField(long id, string name, IFieldSettings? settings = null)
        {
            return Fields.DateTime(id, name, this, settings);
        }
    }
}
