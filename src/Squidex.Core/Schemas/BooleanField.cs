// ==========================================================================
//  BooleanField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using Squidex.Core.Schemas.Validators;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    public sealed class BooleanField : Field<BooleanFieldProperties>
    {
        public BooleanField(long id, string name, BooleanFieldProperties properties)
            : base(id, name, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredValidator();
            }
        }

        protected override object ConvertValue(PropertyValue property)
        {
            return property.ToNullableBoolean(CultureInfo.InvariantCulture);
        }

        protected override Field Clone()
        {
            return new BooleanField(Id, Name, Properties);
        }
    }
}
