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
using NodaTime;
using NodaTime.Text;
using Squidex.Core.Schemas.Validators;
using Squidex.Infrastructure;

// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable ConvertIfStatementToSwitchStatement

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
                yield return new RangeValidator<Instant>(Properties.MinValue, Properties.MaxValue);
            }
        }

        public override object ConvertValue(JToken value)
        {
            if (value.Type == JTokenType.String)
            {
                var parseResult = InstantPattern.General.Parse(value.ToString());

                if (!parseResult.Success)
                {
                    throw parseResult.Exception;
                }

                return parseResult.Value;
            }

            throw new InvalidCastException("Invalid json type, expected string.");
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.String;

            jsonProperty.Format = JsonFormatStrings.DateTime;
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.DateTimeOffset, !Properties.IsRequired);
        }
    }
}
