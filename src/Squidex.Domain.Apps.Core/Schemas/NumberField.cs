// ==========================================================================
//  NumberField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Newtonsoft.Json.Linq;
using NJsonSchema;
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

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.Number;

            if (Properties.MinValue.HasValue)
            {
                jsonProperty.Minimum = (decimal)Properties.MinValue.Value;
            }

            if (Properties.MaxValue.HasValue)
            {
                jsonProperty.Maximum = (decimal)Properties.MaxValue.Value;
            }
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Double, !Properties.IsRequired);
        }

        public override object ConvertValue(JToken value)
        {
            return (double?)value;
        }
    }
}
