// ==========================================================================
//  SchemaJsonSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas.Json
{
    public sealed class SchemaJsonSerializer
    {
        private readonly FieldRegistry fieldRegistry;
        private readonly JsonSerializer serializer;

        public SchemaJsonSerializer(FieldRegistry fieldRegistry, JsonSerializerSettings serializerSettings)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));
            Guard.NotNull(serializerSettings, nameof(serializerSettings));

            this.fieldRegistry = fieldRegistry;

            serializer = JsonSerializer.Create(serializerSettings);
        }

        public JToken Serialize(Schema schema)
        {
            var model = new JsonSchemaModel { Name = schema.Name, IsPublished = schema.IsPublished, Properties = schema.Properties };

            model.Fields =
                schema.Fields.Select(x =>
                    new JsonFieldModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsHidden = x.IsHidden,
                        IsLocked = x.IsLocked,
                        IsDisabled = x.IsDisabled,
                        Partitioning = x.Paritioning.Key,
                        Properties = x.RawProperties
                    }).ToList();

            return JToken.FromObject(model, serializer);
        }

        public Schema Deserialize(JToken token)
        {
            var model = token.ToObject<JsonSchemaModel>(serializer);

            var fields =
                model.Fields.Select(fieldModel =>
                {
                    var parititonKey = new Partitioning(fieldModel.Partitioning);

                    var field = fieldRegistry.CreateField(fieldModel.Id, fieldModel.Name, parititonKey, fieldModel.Properties);

                    if (fieldModel.IsDisabled)
                    {
                        field = field.Disable();
                    }

                    if (fieldModel.IsLocked)
                    {
                        field = field.Lock();
                    }

                    if (fieldModel.IsHidden)
                    {
                        field = field.Hide();
                    }

                    return field;
                }).ToImmutableList();

            var schema =
                new Schema(
                    model.Name,
                    model.IsPublished, model.Properties, fields);

            return schema;
        }
    }
}