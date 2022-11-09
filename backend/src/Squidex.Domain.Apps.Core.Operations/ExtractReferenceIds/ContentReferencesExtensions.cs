// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds;

public static class ContentReferencesExtensions
{
    public static bool CanHaveReference(this ContentData source)
    {
        if (source.Count == 0)
        {
            return false;
        }

        static bool CanValueHaveReference(JsonValue value)
        {
            if (value.Value is JsonArray)
            {
                return true;
            }

            if (value.Value is JsonObject o)
            {
                foreach (var (_, nested) in o)
                {
                    if (CanValueHaveReference(nested))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        foreach (var (_, field) in source)
        {
            if (field != null)
            {
                foreach (var (_, value) in field)
                {
                    if (CanValueHaveReference(value))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static void AddReferencedIds(this ContentData source, Schema schema, HashSet<DomainId> result,
        ResolvedComponents components, int take = int.MaxValue)
    {
        Guard.NotNull(schema);
        Guard.NotNull(result);
        Guard.NotNull(components);

        ReferencesExtractor.Extract(schema.Fields, source, result, take, components);
    }

    public static void AddReferencedIds(this ContentData source, IEnumerable<IField> fields, HashSet<DomainId> result,
        ResolvedComponents components, int take = int.MaxValue)
    {
        Guard.NotNull(fields);
        Guard.NotNull(result);
        Guard.NotNull(components);

        ReferencesExtractor.Extract(fields, source, result, take, components);
    }

    public static void AddReferencedIds(this JsonValue value, IField field, HashSet<DomainId> result,
        ResolvedComponents components, int take = int.MaxValue)
    {
        Guard.NotNull(field);
        Guard.NotNull(result);
        Guard.NotNull(components);

        ReferencesExtractor.Extract(field, value, result, take, components);
    }

    public static HashSet<DomainId> GetReferencedIds(this IField field, JsonValue value,
        ResolvedComponents components, int take = int.MaxValue)
    {
        var result = new HashSet<DomainId>();

        AddReferencedIds(value, field, result, components, take);

        return result;
    }

    public static JsonObject FormatReferences(this ContentData data, Schema schema, IFieldPartitioning partitioning, string separator = ", ")
    {
        Guard.NotNull(schema);
        Guard.NotNull(partitioning);

        var result = new JsonObject();

        foreach (var partitionKey in partitioning.AllKeys)
        {
            result[partitionKey] = data.FormatReferenceFields(schema, partitionKey, separator);
        }

        return result;
    }

    private static string FormatReferenceFields(this ContentData data, Schema schema, string partitionKey, string separator)
    {
        Guard.NotNull(schema);

        var sb = new StringBuilder();

        void AddValue(object value)
        {
            sb.AppendIfNotEmpty(separator);
            sb.Append(value);
        }

        var referenceFields = schema.ReferenceFields();

        foreach (var referenceField in referenceFields)
        {
            if (data.TryGetValue(referenceField.Name, out var fieldData) && fieldData != null)
            {
                if (fieldData.TryGetValue(partitionKey, out var value))
                {
                    AddValue(value);
                }
                else if (fieldData.TryGetValue(InvariantPartitioning.Key, out var value2))
                {
                    AddValue(value2);
                }
            }
        }

        return sb.ToString();
    }
}
