// ==========================================================================
//  NumberField.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.Tasks;

namespace PinkParrot.Core.Schema
{
    public sealed class NumberField : ModelField
    {
        private double? maxValue;
        private double? minValue;

        public double? MaxValue
        {
            get { return maxValue; }
        }

        public double? MinValue
        {
            get { return minValue; }
        }

        public NumberField(Guid id, string name) 
            : base(id, name)
        {
        }

        protected override void ConfigureCore(PropertiesBag settings, ICollection<string> errors)
        {
            maxValue = ParseNumber("MaxValue", settings, errors);
            minValue = ParseNumber("MinValue", settings, errors);

            if (maxValue.HasValue && minValue.HasValue && minValue.Value > maxValue.Value)
            {
                errors.Add("MinValue cannot be larger than max value");
            }
        }

        private static double? ParseNumber(string key, PropertiesBag settings, ICollection<string> errors)
        {
            try
            {
                if (settings.Contains(key))
                {
                    return settings[key].ToNullableDouble(CultureInfo.InvariantCulture);
                }
            }
            catch (InvalidCastException)
            {
                errors.Add($"'{key}' is not a valid number");
            }

            return null;
        }

        protected override Task ValidateCoreAsync(PropertyValue property, ICollection<string> errors)
        {
            try
            {
                var value = property.ToDouble(CultureInfo.InvariantCulture);

                if (MinValue.HasValue && value < MinValue.Value)
                {
                    errors.Add($"<Field> must be greater than {MinValue}");
                }

                if (MaxValue.HasValue && value > MaxValue.Value)
                {
                    errors.Add($"<Field> must be less than {MaxValue}");
                }
            }
            catch (InvalidCastException)
            {
                errors.Add("<Field> is not a valid number");
            }

            return TaskHelper.Done;
        }

        protected override ModelField Clone()
        {
            return (ModelField)MemberwiseClone();
        }
    }
}
