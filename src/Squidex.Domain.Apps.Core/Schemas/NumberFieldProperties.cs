// ==========================================================================
//  NumberFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName(nameof(NumberField))]
    public sealed class NumberFieldProperties : FieldProperties
    {
        private double? maxValue;
        private double? minValue;
        private double? defaultValue;
        private ImmutableList<double> allowedValues;
        private NumberFieldEditor editor;

        public double? MaxValue
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

        public double? MinValue
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

        public double? DefaultValue
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

        public ImmutableList<double> AllowedValues
        {
            get
            {
                return allowedValues;
            }
            set
            {
                ThrowIfFrozen();

                allowedValues = value;
            }
        }

        public NumberFieldEditor Editor
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
            return DefaultValue;
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (!Editor.IsEnumValue())
            {
                yield return new ValidationError("Editor is not a valid value", nameof(Editor));
            }

            if ((Editor == NumberFieldEditor.Radio || Editor == NumberFieldEditor.Dropdown) && (AllowedValues == null || AllowedValues.Count == 0))
            {
                yield return new ValidationError("Radio buttons or dropdown list need allowed values", nameof(AllowedValues));
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

            if (AllowedValues != null && AllowedValues.Count > 0 && (MinValue.HasValue || MaxValue.HasValue))
            {
                yield return new ValidationError("Either allowed values or min and max value can be defined",
                    nameof(AllowedValues),
                    nameof(MinValue),
                    nameof(MaxValue));
            }
        }
    }
}
