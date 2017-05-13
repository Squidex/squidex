// ==========================================================================
//  ContentEnricher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Core
{
    public sealed class ContentEnricher
    {
        private readonly Schema schema;
        private readonly PartitionResolver partitionResolver;

        public ContentEnricher(Schema schema, PartitionResolver partitionResolver)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.schema = schema;

            this.partitionResolver = partitionResolver;
        }

        public void Enrich(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var field in schema.FieldsByName.Values)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());
                var fieldPartition = partitionResolver(field.Paritioning);

                foreach (var partitionItem in fieldPartition)
                {
                    Enrich(field, fieldData, partitionItem);
                }

                if (fieldData.Count > 0)
                {
                    data.AddField(field.Name, fieldData);
                }
            }
        }

        private static void Enrich(Field field, ContentFieldData fieldData, IFieldPartitionItem partitionItem)
        {
            Guard.NotNull(fieldData, nameof(fieldData));

            var defaultValue = field.RawProperties.GetDefaultValue();

            if (field.RawProperties.IsRequired || defaultValue.IsNull())
            {
                return;
            }

            var key = partitionItem.Key;

            if (!fieldData.TryGetValue(key, out JToken value) || value == null || value.Type == JTokenType.Null)
            {
                fieldData.AddValue(key, defaultValue);
            }
        }
    }
}
