// ==========================================================================
//  NumberField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class NumberField : Field<NumberFieldProperties>
    {
        public NumberField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new NumberFieldProperties())
        {
        }

        public NumberField(long id, string name, Partitioning partitioning, NumberFieldProperties properties)
            : base(id, name, partitioning, properties)
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

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override object ConvertValue(JToken value)
        {
            return (double?)value;
        }
    }
}
