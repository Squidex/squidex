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

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class AddDefaultValues : IContentDataConverter, IContentItemConverter
{
    private readonly PartitionResolver partitionResolver;
    private readonly bool ignoreRequiredFields;
    private readonly IClock clock;
    private Instant now;

    public AddDefaultValues(PartitionResolver partitionResolver, bool ignoreRequiredFields, IClock? clock = null)
    {
        this.partitionResolver = partitionResolver;
        this.ignoreRequiredFields = ignoreRequiredFields;
        this.clock = clock ?? SystemClock.Instance;
    }

    public void ConvertDataAfter(Schema schema, ContentData data)
    {
        foreach (var field in schema.Fields)
        {
            var fieldData = data.GetOrCreate(field.Name, _ => new ContentFieldData()) ?? new ContentFieldData();

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

    public JsonObject ConvertItemBefore(IField parentField, JsonObject item, IEnumerable<IField> schema)
    {
        foreach (var field in schema)
        {
            Enrich(field, item, field.Name);
        }

        return item;
    }

    private void Enrich(IField field, Dictionary<string, JsonValue> fieldData, string key)
    {
        if (fieldData.TryGetValue(key, out _))
        {
            return;
        }

        var defaultValue = DefaultValueFactory.CreateDefaultValue(field, GetNow(), key);

        if ((field.RawProperties.IsRequired && ignoreRequiredFields) || defaultValue == default)
        {
            return;
        }

        fieldData[key] = defaultValue;
    }

    private Instant GetNow()
    {
        if (now == default)
        {
            now = clock.GetCurrentInstant();
        }

        return now;
    }
}
