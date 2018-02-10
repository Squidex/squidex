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
    [TypeName(nameof(DateTimeField))]
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

        public override Field CreateField(long id, string name, Partitioning partitioning)
        {
            return new DateTimeField(id, name, partitioning, this);
        }
    }
}
