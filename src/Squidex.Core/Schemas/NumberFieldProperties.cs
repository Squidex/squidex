// ==========================================================================
//  NumberFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    [TypeName("number")]
    public sealed class NumberFieldProperties : FieldProperties
    {
        public string Placeholder { get; set; }

        public double? MaxValue { get; set; }

        public double? MinValue { get; set; }

        public double? DefaultValue { get; set; }

        public ImmutableList<double> AllowedValues { get; set; }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            if (MaxValue.HasValue && MinValue.HasValue && MinValue.Value >= MaxValue.Value)
            {
                yield return new ValidationError("Max value must be greater than min value.", nameof(MinValue), nameof(MaxValue));
            }

            if (AllowedValues != null && (MinValue.HasValue || MaxValue.HasValue))
            {
                yield return new ValidationError("Either or allowed values or range can be defined.",
                    nameof(AllowedValues),
                    nameof(MinValue),
                    nameof(MaxValue));
            }

            if (!DefaultValue.HasValue)
            {
                yield break;
            }

            if (MinValue.HasValue && DefaultValue.Value < MinValue.Value)
            {
                yield return new ValidationError("Default value must be greater than min value.", nameof(DefaultValue));
            }

            if (MaxValue.HasValue && DefaultValue.Value > MaxValue.Value)
            {
                yield return new ValidationError("Default value must be less than max value.", nameof(DefaultValue));
            }
        }
    }
}
