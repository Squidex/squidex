// ==========================================================================
//  DateTimeFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
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

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (!Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value", nameof(Editor));
            }

            if (MaxValue.HasValue && MinValue.HasValue && MinValue.Value >= MaxValue.Value)
            {
                yield return new ValidationError("Max value must be greater than min value", nameof(MinValue), nameof(MaxValue));
            }

            if (DefaultValue.HasValue && MinValue.HasValue && DefaultValue.Value < MinValue.Value)
            {
                yield return new ValidationError("Default value must be greater than min value", nameof(DefaultValue));
            }

            if (DefaultValue.HasValue && MaxValue.HasValue && DefaultValue.Value > MaxValue.Value)
            {
                yield return new ValidationError("Default value must be less than max value", nameof(DefaultValue));
            }

            if (CalculatedDefaultValue.HasValue)
            {
                if (!CalculatedDefaultValue.Value.IsEnumValue())
                {
                    yield return new ValidationError("Calculated default value is not valid", nameof(CalculatedDefaultValue));
                }

                if (DefaultValue.HasValue)
                {
                    yield return new ValidationError("Calculated default value and default value cannot be used together", nameof(CalculatedDefaultValue), nameof(DefaultValue));
                }
            }
        }
    }
}
