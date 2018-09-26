// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("DateTimeField")]
    public sealed class DateTimeFieldProperties : FieldProperties
    {
        public Instant? MaxValue { get; set; }

        public Instant? MinValue { get; set; }

        public Instant? DefaultValue { get; set; }

        public DateTimeCalculatedDefaultValue? CalculatedDefaultValue { get; set; }

        public DateTimeFieldEditor Editor { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override T Accept<T>(IFieldVisitor<T> visitor, IField field)
        {
            return visitor.Visit((IField<DateTimeFieldProperties>)field);
        }

        public override RootField CreateRootField(long id, string name, Partitioning partitioning)
        {
            return Fields.DateTime(id, name, partitioning, this);
        }

        public override NestedField CreateNestedField(long id, string name)
        {
            return Fields.DateTime(id, name, this);
        }
    }
}
