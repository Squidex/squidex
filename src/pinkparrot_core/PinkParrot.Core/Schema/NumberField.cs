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

        public NumberField(long id, string name) 
            : base(id, name)
        {
        }

        protected override void ConfigureCore(dynamic settings, ICollection<string> errors)
        {
            maxValue = settings.MaxValue;
            minValue = settings.MinValue;

            if (maxValue.HasValue && minValue.HasValue && minValue.Value > maxValue.Value)
            {
                errors.Add("MinValue cannot be larger than max value");
            }
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
