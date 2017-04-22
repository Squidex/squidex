// ==========================================================================
//  AssetsField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Core.Schemas.Validators;

namespace Squidex.Core.Schemas
{
    public sealed class AssetsField : Field<AssetsFieldProperties>
    {
        private readonly IAssetTester assetTester;

        public AssetsField(long id, string name, AssetsFieldProperties properties, IAssetTester assetTester)
            : base(id, name, properties)
        {
            this.assetTester = assetTester;
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            yield return new AssetsValidator(assetTester, Properties.IsRequired);
        }

        public override object ConvertValue(JToken value)
        {
            return new AssetsValue(value.ToObject<Guid[]>());
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            var itemSchema = schemaResolver("AssetItem", new JsonSchema4 { Type = JsonObjectType.String });

            jsonProperty.Type = JsonObjectType.Array;
            jsonProperty.Item = itemSchema;
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            return null;
        }
    }
}
