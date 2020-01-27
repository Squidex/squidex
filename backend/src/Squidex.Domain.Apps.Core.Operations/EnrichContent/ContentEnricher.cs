﻿// ==========================================================================
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

namespace Squidex.Domain.Apps.Core.EnrichContent
{
    public sealed class ContentEnricher
    {
        private readonly Schema schema;
        private readonly PartitionResolver partitionResolver;

        public ContentEnricher(Schema schema, PartitionResolver partitionResolver)
        {
            Guard.NotNull(schema);
            Guard.NotNull(partitionResolver);

            this.schema = schema;

            this.partitionResolver = partitionResolver;
        }

        public void Enrich(NamedContentData data)
        {
            Guard.NotNull(data);

            foreach (var field in schema.Fields)
            {
                var fieldData = data.GetOrCreate(field.Name, k => new ContentFieldData());

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
            Guard.NotNull(fieldData);

            var defaultValue = DefaultValueFactory.CreateDefaultValue(field, SystemClock.Instance.GetCurrentInstant());

            if (field.RawProperties.IsRequired || defaultValue == null || defaultValue.Type == JsonValueType.Null)
            {
                return;
            }

            if (!fieldData.TryGetValue(partitionKey, out var value) || ShouldApplyDefaultValue(field, value))
            {
                fieldData.AddJsonValue(partitionKey, defaultValue);
            }
        }

        private static bool ShouldApplyDefaultValue(IField field, IJsonValue value)
        {
            return value.Type == JsonValueType.Null || (field is IField<StringFieldProperties> && value is JsonScalar<string> s && string.IsNullOrEmpty(s.Value));
        }
    }
}
