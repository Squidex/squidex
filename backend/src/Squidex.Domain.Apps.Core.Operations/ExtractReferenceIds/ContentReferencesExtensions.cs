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

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ContentReferencesExtensions
    {
        public static bool CanHaveReference(this ContentData source)
        {
            if (source.Count == 0)
            {
                return false;
            }

            static bool CanHaveReference(IJsonValue value)
            {
                if (value is JsonArray)
                {
                    return true;
                }

                if (value is JsonObject obj)
                {
                    foreach (var nested in obj.Values)
                    {
                        if (CanHaveReference(nested))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return source.Values.NotNull().SelectMany(x => x.Values).Any(CanHaveReference);
        }

        public static HashSet<DomainId> GetReferencedIds(this ContentData source, Schema schema,
            ResolvedComponents components, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(schema);

            var ids = new HashSet<DomainId>();

            AddReferencedIds(source, schema, ids, components, referencesPerField);

            return ids;
        }

        public static void AddReferencedIds(this ContentData source, Schema schema, HashSet<DomainId> result,
            ResolvedComponents components, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(schema);

            AddReferencedIds(source, schema.Fields, result, components, referencesPerField);
        }

        public static void AddReferencedIds(this ContentData source, IEnumerable<IField> fields, HashSet<DomainId> result,
            ResolvedComponents components, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(fields);
            Guard.NotNull(result);
            Guard.NotNull(components);

            foreach (var field in fields)
            {
                AddReferencedIds(field, source, result, components, referencesPerField);
            }
        }

        private static void AddReferencedIds(IField field, ContentData source, HashSet<DomainId> result,
            ResolvedComponents components, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(components);

            if (source.TryGetValue(field.Name, out var fieldData) && fieldData != null)
            {
                foreach (var partitionValue in fieldData)
                {
                    ReferencesExtractor.Extract(field, partitionValue.Value, result, referencesPerField, components);
                }
            }
        }

        public static HashSet<DomainId> GetReferencedIds(this IField field, IJsonValue? value,
            ResolvedComponents components, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(components);

            var result = new HashSet<DomainId>();

            if (value != null)
            {
                ReferencesExtractor.Extract(field, value, result, referencesPerField, components);
            }

            return result;
        }

        public static JsonObject FormatReferences(this ContentData data, Schema schema, IFieldPartitioning partitioning, string separator = ", ")
        {
            Guard.NotNull(schema);
            Guard.NotNull(partitioning);

            var result = JsonValue.Object();

            foreach (var partitionKey in partitioning.AllKeys)
            {
                result[partitionKey] = JsonValue.Create(data.FormatReferenceFields(schema, partitionKey, separator));
            }

            return result;
        }

        private static string FormatReferenceFields(this ContentData data, Schema schema, string partitionKey, string separator)
        {
            Guard.NotNull(schema);

            var sb = new StringBuilder();

            void AddValue(object value)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }

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
}
