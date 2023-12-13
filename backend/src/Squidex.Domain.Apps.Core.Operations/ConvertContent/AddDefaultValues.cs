// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ConvertContent;

public sealed class AddDefaultValues : IContentDataConverter, IContentItemConverter, IContentFieldConverter
{
    private readonly PartitionResolver partitionResolver;
    private readonly IClock clock;
    private Instant now;

    public bool IgnoreRequiredFields { get; init; }

    public bool IgnoreNonMasterFields { get; init; }

    public HashSet<string>? FieldNames { get; init; }

    public AddDefaultValues(PartitionResolver partitionResolver, IClock? clock = null)
    {
        this.partitionResolver = partitionResolver;

        this.clock = clock ?? SystemClock.Instance;
    }

    public void ConvertDataBefore(Schema schema, ContentData data)
    {
        foreach (var field in schema.Fields)
        {
            // If the fields are set, we only enrich the given matching field names.
            if (FieldNames?.Contains(field.Name) == false)
            {
                continue;
            }

            if (data.TryGetValue(field.Name, out var fieldData) && fieldData != null)
            {
                continue;
            }

            if ((field.RawProperties.IsRequired && IgnoreRequiredFields) || !DefaultValueChecker.HasDefaultValue(field))
            {
                continue;
            }

            data[field.Name] = [];
        }
    }

    public ContentFieldData? ConvertFieldAfter(IRootField field, ContentFieldData source)
    {
        var partitioning = partitionResolver(field.Partitioning);

        foreach (var partitionKey in partitioning.AllKeys)
        {
            // If the fields are set, we only enrich the given matching field names.
            if (FieldNames?.Contains(field.Name) == false)
            {
                continue;
            }

            if (!partitioning.IsMaster(partitionKey) && IgnoreNonMasterFields)
            {
                continue;
            }

            Enrich(field, source, partitionKey);
        }

        return source;
    }

    public JsonObject ConvertItemBefore(IField parentField, JsonObject source, IEnumerable<IField> schema)
    {
        foreach (var field in schema)
        {
            Enrich(field, source, field.Name);
        }

        return source;
    }

    private void Enrich(IField field, Dictionary<string, JsonValue> fieldData, string key)
    {
        if (fieldData.TryGetValue(key, out _) || (field.RawProperties.IsRequired && IgnoreRequiredFields))
        {
            return;
        }

        var defaultValue = DefaultValueFactory.CreateDefaultValue(field, GetNow(), key);

        if (defaultValue == default)
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
