// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class JsonSchemaModel
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public bool IsPublished { get; set; }

        [JsonProperty]
        public SchemaProperties Properties { get; set; }

        [JsonProperty]
        public JsonFieldModel[] Fields { get; set; }

        public JsonSchemaModel()
        {
        }

        public JsonSchemaModel(Schema schema)
        {
            Name = schema.Name;

            Properties = schema.Properties;

            Fields =
                schema.Fields.ToArray(x =>
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
                    });

            IsPublished = schema.IsPublished;
        }

        private static JsonNestedFieldModel[] CreateChildren(IField field)
        {
            if (field is ArrayField arrayField)
            {
                return arrayField.Fields.ToArray(x =>
                    new JsonNestedFieldModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsHidden = x.IsHidden,
                        IsDisabled = x.IsDisabled,
                        Properties = x.RawProperties
                    });
            }

            return null;
        }

        public Schema ToSchema()
        {
            var fields = Fields.ToArray(f => f.ToField()) ?? Array.Empty<RootField>();

            return new Schema(Name, fields, Properties, IsPublished);
        }
    }
}