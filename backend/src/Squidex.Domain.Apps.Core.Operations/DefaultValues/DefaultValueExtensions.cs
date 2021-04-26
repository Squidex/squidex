// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.DefaultValues
{
    public static class DefaultValueExtensions
    {
        public static void GenerateDefaultValues(this ContentData data, Schema schema, PartitionResolver partitionResolver)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

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
            var defaultValue = DefaultValueFactory.CreateDefaultValue(field, SystemClock.Instance.GetCurrentInstant(), partitionKey);

            if (field.RawProperties.IsRequired || defaultValue == null || defaultValue.Type == JsonValueType.Null)
            {
                return;
            }

            if (!fieldData.TryGetValue(partitionKey, out _))
            {
                fieldData.AddLocalized(partitionKey, defaultValue);
            }
        }
    }
}
