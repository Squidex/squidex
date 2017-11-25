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
        private DateTimeFieldEditor editor;
        private DateTimeCalculatedDefaultValue? calculatedDefaultValue;
        private Instant? maxValue;
        private Instant? minValue;
        private Instant? defaultValue;

        public Instant? MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                ThrowIfFrozen();

                maxValue = value;
            }
        }

        public Instant? MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                ThrowIfFrozen();

                minValue = value;
            }
        }

        public Instant? DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                ThrowIfFrozen();

                defaultValue = value;
            }
        }

        public DateTimeCalculatedDefaultValue? CalculatedDefaultValue
        {
            get
            {
                return calculatedDefaultValue;
            }
            set
            {
                ThrowIfFrozen();

                calculatedDefaultValue = value;
            }
        }

        public DateTimeFieldEditor Editor
        {
            get
            {
                return editor;
            }
            set
            {
                ThrowIfFrozen();

                editor = value;
            }
        }

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
