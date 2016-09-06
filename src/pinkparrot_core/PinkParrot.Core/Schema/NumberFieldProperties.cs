// ==========================================================================
//  NumberFieldProperties.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using PinkParrot.Infrastructure;

namespace PinkParrot.Core.Schema
{
    [TypeName("Number")]
    public sealed class NumberFieldProperties : ModelFieldProperties
    {
        public double? DefaultValue { get; }

        public double? MaxValue { get; }

        public double? MinValue { get; }

        public string Placeholder { get; set; }

        public NumberFieldProperties(
            bool isRequired,
            string name,
            string label,
            string hints,
            string placeholder,
            double? minValue,
            double? maxValue,
            double? defaultValue)
            : base(isRequired, name, label, hints)
        {
            Placeholder = placeholder;

            MinValue = minValue;
            MaxValue = maxValue;

            DefaultValue = defaultValue;
        }

        protected override void ValidateCore(IList<ValidationError> errors)
        {
            if (MaxValue.HasValue && MinValue.HasValue)
            {
                errors.Add(new ValidationError("MinValue cannot be larger than max value", "MinValue", "MaxValue"));
            }

            if (DefaultValue.HasValue)
            {
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
}
