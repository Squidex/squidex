// ==========================================================================
//  DateTimeFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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

        public DateTimeFieldEditor Editor { get; set; }

        public DateTimeCalculatedDefaultValue? CalculatedDefaultValue { get; set; }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
