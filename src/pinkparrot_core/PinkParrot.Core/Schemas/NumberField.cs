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
using System.Linq;
using System.Threading.Tasks;
using PinkParrot.Infrastructure;
using PinkParrot.Infrastructure.Tasks;

namespace PinkParrot.Core.Schemas
{
    public sealed class NumberField : Field<NumberFieldProperties>
    {
        public double? MaxValue
        {
            get { return Properties.MaxValue; }
        }

        public double? MinValue
        {
            get { return Properties.MinValue; }
        }

        public double[] AllowedValues
        {
            get { return Properties.AllowedValues; }
        }

        public NumberField(long id, string name, NumberFieldProperties properties) 
            : base(id, name, properties)
        {
        }

        protected override Task ValidateCoreAsync(PropertyValue property, ICollection<string> errors)
        {
            try
            {
                var value = property.ToDouble(CultureInfo.InvariantCulture);

                if (MinValue.HasValue && value < MinValue.Value)
                {
                    errors.Add($"Must be greater than {MinValue}");
                }

                if (MaxValue.HasValue && value > MaxValue.Value)
                {
                    errors.Add($"Must be less than {MaxValue}");
                }

                if (AllowedValues != null && !AllowedValues.Contains(value))
                {
                    errors.Add($"Can only be one of the following value: {string.Join(", ", AllowedValues)}");
                }
            }
            catch (InvalidCastException)
            {
                errors.Add("Value is not a valid number");
            }

            return TaskHelper.Done;
        }

        protected override Field Clone()
        {
            return (Field)MemberwiseClone();
        }
    }
}
