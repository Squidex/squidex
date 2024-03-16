// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed record DateTimeFieldProperties : FieldProperties
    {
        public LocalizedValue<Instant?> DefaultValues { get; init; }

        public Instant? DefaultValue { get; init; }

        public Instant? MaxValue { get; init; }

        public Instant? MinValue { get; init; }

        public string? Format { get; set; }

        public DateTimeCalculatedDefaultValue? CalculatedDefaultValue { get; init; }

        public DateTimeFieldEditor Editor { get; init; }

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
