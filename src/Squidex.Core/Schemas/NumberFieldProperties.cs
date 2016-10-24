// ==========================================================================
//  NumberFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    [TypeName("number")]
    public sealed class NumberFieldProperties : FieldProperties
    {
        public string Placeholder { get; set; }

        public double? DefaultValue { get; }

        public double? MaxValue { get; }

        public double? MinValue { get; }

        public double[] AllowedValues { get; }

        public NumberFieldProperties(
            string label,
            string hints,
            string placeholder,
            double? minValue,
            double? maxValue,
            double? defaultValue,
            double[] allowedValues,
            bool isRequired)
            : base(label, hints, isRequired)
        {
            Placeholder = placeholder;

            MinValue = minValue;
            MaxValue = maxValue;

            AllowedValues = allowedValues;
            
            DefaultValue = defaultValue;
        }

        protected override void ValidateCore(IList<ValidationError> errors)
        {
            if (MaxValue.HasValue && MinValue.HasValue && MinValue.Value >= MaxValue.Value)
            {
                errors.Add(new ValidationError("MinValue cannot be larger than max value", "MinValue", "MaxValue"));
            }

            if (!DefaultValue.HasValue)
            {
                return;
            }

            if (MinValue.HasValue && DefaultValue.Value < MinValue.Value)
            {
                errors.Add(new ValidationError("DefaultValue must be larger than the min value.", "DefaultValue"));
            }

            if (MaxValue.HasValue && DefaultValue.Value > MaxValue.Value)
            {
                errors.Add(new ValidationError("DefaultValue must be smaller than the max value.", "DefaultValue"));
            }
        }
    }
}
