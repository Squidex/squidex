// ==========================================================================
//  DateTimeField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Core.Schemas.Validators;
using Squidex.Infrastructure;

namespace Squidex.Core.Schemas
{
    [TypeName("DateTimeField")]
    public sealed class DateTimeField : Field<DateTimeFieldProperties>
    {
        public DateTimeField(long id, string name, DateTimeFieldProperties properties) 
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
                yield return new RangeValidator<DateTimeOffset>(Properties.MinValue, Properties.MaxValue);
            }
        }

        protected override object ConvertValue(JToken value)
        {
            return (DateTimeOffset?)value;
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty)
        {
            jsonProperty.Type = JsonObjectType.String;
            jsonProperty.Format = "date-time";
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.DateTimeOffset, !Properties.IsRequired);
        }
    }
}
