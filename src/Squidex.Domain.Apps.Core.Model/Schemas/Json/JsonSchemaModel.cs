// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class JsonSchemaModel
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Category { get; set; }

        [JsonProperty]
        public bool IsSingleton { get; set; }

        [JsonProperty]
        public bool IsPublished { get; set; }

        [JsonProperty]
        public SchemaProperties Properties { get; set; }

        [JsonProperty]
        public SchemaScripts Scripts { get; set; }

        [JsonProperty]
        public JsonFieldModel[] Fields { get; set; }

        [JsonProperty]
        public Dictionary<string, string> PreviewUrls { get; set; }

        public JsonSchemaModel()
        {
        }

        public JsonSchemaModel(Schema schema)
        {
            SimpleMapper.Map(schema, this);

            Fields =
                schema.Fields.Select(x =>
                    new JsonFieldModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Children = CreateChildren(x),
                        IsHidden = x.IsHidden,
                        IsLocked = x.IsLocked,
                        IsDisabled = x.IsDisabled,
                        Partitioning = x.Partitioning.Key,
                        Properties = x.RawProperties
                    }).ToArray();

            PreviewUrls = schema.PreviewUrls.ToDictionary(x => x.Key, x => x.Value);
        }

        private static JsonNestedFieldModel[] CreateChildren(IField field)
        {
            if (field is ArrayField arrayField)
            {
                return arrayField.Fields.Select(x =>
                    new JsonNestedFieldModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsHidden = x.IsHidden,
                        IsLocked = x.IsLocked,
                        IsDisabled = x.IsDisabled,
                        Properties = x.RawProperties
                    }).ToArray();
            }

            return null;
        }

        public Schema ToSchema()
        {
            var fields = Fields.Map(f => f.ToField()) ?? Array.Empty<RootField>();

            var schema = new Schema(Name, fields, Properties, IsPublished, IsSingleton);

            if (!string.IsNullOrWhiteSpace(Category))
            {
                schema = schema.ChangeCategory(Category);
            }

            if (Scripts != null)
            {
                schema = schema.ConfigureScripts(Scripts);
            }

            if (PreviewUrls?.Count > 0)
            {
                schema = schema.ConfigurePreviewUrls(PreviewUrls);
            }

            return schema;
        }
    }
}