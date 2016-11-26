// ==========================================================================
//  NumberField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using Squidex.Core.Schemas.Validators;
using Squidex.Infrastructure;
using System.Linq;

namespace Squidex.Core.Schemas
{
    public sealed class NumberField : Field<NumberFieldProperties>
    {
        public NumberField(long id, string name, NumberFieldProperties properties) 
            : base(id, name, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }

            if (Properties.MinValue.HasValue || Properties.MaxValue.HasValue)
            {
                yield return new RangeValidator<double>(Properties.MinValue, Properties.MaxValue);
            }

            if (Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<double>(Properties.AllowedValues.ToArray());
            }
        }

        protected override object ConvertValue(PropertyValue property)
        {
            return property.ToNullableDouble(CultureInfo.InvariantCulture);
        }

        public override Field Clone()
        {
            return new NumberField(Id, Name, Properties);
        }
    }
}
