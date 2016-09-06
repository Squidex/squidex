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
    public sealed class NumberField : ModelField<NumberFieldProperties>
    {
        public double? MaxValue
        {
            get { return Properties.MaxValue; }
        }

        public double? MinValue
        {
            get { return Properties.MinValue; }
        }

        public NumberField(long id, NumberFieldProperties properties) 
            : base(id, properties)
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
            }
            catch (InvalidCastException)
            {
                errors.Add("Value is not a valid number");
            }

            return TaskHelper.Done;
        }

        protected override ModelField Clone()
        {
            return (ModelField)MemberwiseClone();
        }
    }
}
