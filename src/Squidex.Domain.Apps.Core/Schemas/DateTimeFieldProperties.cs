// ==========================================================================
//  DateTimeFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json.Linq;
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

        public override JToken GetDefaultValue()
        {
            if (CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Now)
            {
                return DateTime.UtcNow.ToString("o");
            }
            else if (CalculatedDefaultValue == DateTimeCalculatedDefaultValue.Today)
            {
                return DateTime.UtcNow.Date.ToString("o");
            }
            else
            {
                return DefaultValue?.ToString();
            }
        }

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
