// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.DefaultValues
{
    public sealed class DefaultValueGenerator
    {
        private readonly Schema schema;
        private readonly PartitionResolver partitionResolver;

        public DefaultValueGenerator(Schema schema, PartitionResolver partitionResolver)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.schema = schema;

            this.partitionResolver = partitionResolver;
        }

        public void Enrich(ContentData data)
        {
            Guard.NotNull(data, nameof(data));

            foreach (var field in schema.Fields)
            {
                var fieldData = data.GetOrCreate(field.Name, _ => new ContentFieldData());

                if (fieldData != null)
                {
                    var partitioning = partitionResolver(field.Partitioning);

                    foreach (var partitionKey in partitioning.AllKeys)
                    {
                        Enrich(field, fieldData, partitionKey);
                    }

                    if (fieldData.Count > 0)
                    {
                        data[field.Name] = fieldData;
                    }
                }
            }
        }

        private static void Enrich(IField field, ContentFieldData fieldData, string partitionKey)
        {
            Guard.NotNull(fieldData, nameof(fieldData));

            var defaultValue = DefaultValueFactory.CreateDefaultValue(field, SystemClock.Instance.GetCurrentInstant(), partitionKey);

            if (field.RawProperties.IsRequired || defaultValue == null || defaultValue.Type == JsonValueType.Null)
            {
                return;
            }

            if (!fieldData.TryGetValue(partitionKey, out var value) || ShouldApplyDefaultValue(field, value))
            {
                fieldData.AddLocalized(partitionKey, defaultValue);
            }
        }

        private static bool ShouldApplyDefaultValue(IField field, IJsonValue value)
        {
            return value.Type == JsonValueType.Null || (field is IField<StringFieldProperties> && value is JsonString s && string.IsNullOrEmpty(s.Value));
        }
    }
}
