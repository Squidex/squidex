// ==========================================================================
//  ContentEnricher.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core
{
    public sealed class ContentEnricher<T>
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

        public void Enrich(ContentData<T> data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var field in schema.Fields)
            {
                var fieldKey = data.GetKey(field);
                var fieldData = data.GetOrCreate(fieldKey, k => new ContentFieldData());
                var fieldPartition = partitionResolver(field.Paritioning);

                foreach (var partitionItem in fieldPartition)
                {
                    Enrich(field, fieldData, partitionItem);
                }

                if (fieldData.Count > 0)
                {
                    data[fieldKey] = fieldData;
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

            if (!fieldData.TryGetValue(key, out var value) || field.RawProperties.ShouldApplyDefaultValue(value))
            {
                fieldData.AddValue(key, defaultValue);
            }
        }
    }
}
