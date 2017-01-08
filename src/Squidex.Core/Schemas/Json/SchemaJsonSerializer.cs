// ==========================================================================
//  SchemaJsonSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Squidex.Core.Schemas.Json
{
    public sealed class SchemaJsonSerializer
    {
        private readonly FieldRegistry fieldRegistry;
        private readonly JsonSerializer serializer;

        public class FieldModel
        {
            public string Name;

            public bool IsHidden;

            public bool IsDisabled;

            public FieldProperties Properties;
        }

        public sealed class SchemaModel
        {
            public string Name;

            public bool IsPublished;

            public SchemaProperties Properties;

            public Dictionary<long, FieldModel> Fields;
        }

        public SchemaJsonSerializer(FieldRegistry fieldRegistry, JsonSerializerSettings serializerSettings)
        {
            Guard.NotNull(fieldRegistry, nameof(fieldRegistry));
            Guard.NotNull(serializerSettings, nameof(serializerSettings));

            this.fieldRegistry = fieldRegistry;

            serializer = JsonSerializer.Create(serializerSettings);
        }

        public JToken Serialize(Schema schema)
        {
            var model = new SchemaModel { Name = schema.Name, IsPublished = schema.IsPublished, Properties = schema.Properties };

            model.Fields =
                schema.Fields
                    .Select(x =>
                        new KeyValuePair<long, FieldModel>(x.Key,
                            new FieldModel
                            {
                                Name = x.Value.Name,
                                IsHidden = x.Value.IsHidden,
                                IsDisabled = x.Value.IsDisabled,
                                Properties = x.Value.RawProperties
                            }))
                    .ToDictionary(x => x.Key, x => x.Value);

            return JToken.FromObject(model, serializer);
        }

        public Schema Deserialize(JToken token)
        {
            var model = token.ToObject<SchemaModel>(serializer);

            var fields =
                model.Fields.Select(kvp =>
                {
                    var fieldModel = kvp.Value;

                    var field = fieldRegistry.CreateField(kvp.Key, fieldModel.Name, fieldModel.Properties);

                    if (fieldModel.IsDisabled)
                    {
                        field = field.Disable();
                    }

                    if (fieldModel.IsHidden)
                    {
                        field = field.Hide();
                    }

                    return field;
                }).ToDictionary(x => x.Id, x => x).ToImmutableDictionary();

            var schema =
                new Schema(
                    model.Name,
                    model.Properties,
                    model.IsPublished,
                    fields);

            return schema;
        }
    }
}