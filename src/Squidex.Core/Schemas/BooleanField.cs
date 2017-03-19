// ==========================================================================
//  BooleanField.cs
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

        public override object ConvertValue(JToken value)
        {
            return (bool?)value;
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.Boolean;
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, !Properties.IsRequired);
        }
    }
}
