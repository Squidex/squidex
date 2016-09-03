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
        public double? MaxValue { get; }

        public double? MinValue { get; }

        public NumberFieldProperties(
            bool isRequired,
            string name,
            string label,
            string hints,
            double? minValue,
            double? maxValue)
            : base(isRequired, name, label, hints)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        protected override void ValidateCore(IList<ValidationError> errors)
        {
            if (MaxValue.HasValue && MinValue.HasValue && MinValue.Value > MaxValue.Value)
            {
                errors.Add(new ValidationError("MinValue cannot be larger than max value", "MinValue", "MaxValue"));
            }
        }
    }
}
