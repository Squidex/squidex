// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.EnrichContent
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

        public void Enrich(NamedContentData data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var field in schema.Fields)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());
                var fieldPartition = partitionResolver(field.Partitioning);

                foreach (var partitionItem in fieldPartition)
                {
                    Enrich(field, fieldData, partitionItem);
                }

                if (fieldData.Count > 0)
                {
                    data[field.Name] = fieldData;
                }
            }
        }

        private static void Enrich(Field field, ContentFieldData fieldData, IFieldPartitionItem partitionItem)
        {
            Guard.NotNull(fieldData, nameof(fieldData));

            var defaultValue = DefaultValueFactory.CreateDefaultValue(field, SystemClock.Instance.GetCurrentInstant());

            if (field.RawProperties.IsRequired || defaultValue.IsNull())
            {
                return;
            }

            var key = partitionItem.Key;

            if (!fieldData.TryGetValue(key, out var value) || ShouldApplyDefaultValue(field, value))
            {
                fieldData.AddValue(key, defaultValue);
            }
        }

        private static bool ShouldApplyDefaultValue(Field field, JToken value)
        {
            return value.IsNull() || (field is StringField && value is JValue jValue && Equals(jValue.Value, string.Empty));
        }
    }
}
