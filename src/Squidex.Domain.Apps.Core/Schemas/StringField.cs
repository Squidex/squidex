// ==========================================================================
//  StringField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class StringField : Field<StringFieldProperties>
    {
        public StringField(long id, string name, Partitioning partitioning)
            : this(id, name, partitioning, new StringFieldProperties())
        {
        }

        public StringField(long id, string name, Partitioning partitioning, StringFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.IsRequired)
            {
                yield return new RequiredStringValidator();
            }

            if (Properties.MinLength.HasValue || Properties.MaxLength.HasValue)
            {
                yield return new StringLengthValidator(Properties.MinLength, Properties.MaxLength);
            }

            if (!string.IsNullOrWhiteSpace(Properties.Pattern))
            {
                yield return new PatternValidator(Properties.Pattern, Properties.PatternMessage);
            }

            if (Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidator<string>(Properties.AllowedValues.ToArray());
            }
        }

        public override object ConvertValue(JToken value)
        {
            return value.ToString();
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.String;

            jsonProperty.MinLength = Properties.MinLength;
            jsonProperty.MaxLength = Properties.MaxLength;

            if (Properties.AllowedValues != null)
            {
                var names = jsonProperty.EnumerationNames = jsonProperty.EnumerationNames ?? new Collection<string>();

                foreach (var value in Properties.AllowedValues)
                {
                    names.Add(value);
                }
            }
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, !Properties.IsRequired);
        }
    }
}
