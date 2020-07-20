// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ContentReferencesExtensions
    {
        public static HashSet<DomainId> GetReferencedIds(this NamedContentData source, Schema schema, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(schema, nameof(schema));

            var extractor = new ReferencesExtractor(new HashSet<DomainId>(), referencesPerField);

            AddReferencedIds(source, schema.Fields, extractor);

            return extractor.Result;
        }

        public static void AddReferencedIds(this NamedContentData source, Schema schema, HashSet<DomainId> result, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(schema, nameof(schema));

            var extractor = new ReferencesExtractor(result, referencesPerField);

            AddReferencedIds(source, schema.Fields, extractor);
        }

        public static void AddReferencedIds(this NamedContentData source, IEnumerable<IField> fields, HashSet<DomainId> result, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(fields, nameof(fields));

            var extractor = new ReferencesExtractor(result, referencesPerField);

            AddReferencedIds(source, fields, extractor);
        }

        public static void AddReferencedIds(this NamedContentData source, IField field, HashSet<DomainId> result, int referencesPerField = int.MaxValue)
        {
            Guard.NotNull(field, nameof(field));

            var extractor = new ReferencesExtractor(result, referencesPerField);

            AddReferencedIds(source, field, extractor);
        }

        private static void AddReferencedIds(NamedContentData source, IEnumerable<IField> fields, ReferencesExtractor extractor)
        {
            foreach (var field in fields)
            {
                AddReferencedIds(source, field, extractor);
            }
        }

        private static void AddReferencedIds(NamedContentData source, IField field, ReferencesExtractor extractor)
        {
            if (source.TryGetValue(field.Name, out var fieldData) && fieldData != null)
            {
                foreach (var partitionValue in fieldData)
                {
                    extractor.SetValue(partitionValue.Value);

                    field.Accept(extractor);
                }
            }
        }

        public static HashSet<DomainId> GetReferencedIds(this IField field, IJsonValue? value, int referencesPerField = int.MaxValue)
        {
            var result = new HashSet<DomainId>();

            if (value != null)
            {
                var extractor = new ReferencesExtractor(result, referencesPerField);

                extractor.SetValue(value);

                field.Accept(extractor);
            }

            return result;
        }

        public static JsonObject FormatReferences(this NamedContentData data, Schema schema, IFieldPartitioning partitioning, string separator = ", ")
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(partitioning, nameof(partitioning));

            var result = JsonValue.Object();

            foreach (var partitionKey in partitioning.AllKeys)
            {
                result[partitionKey] = JsonValue.Create(data.FormatReferenceFields(schema, partitionKey, separator));
            }

            return result;
        }

        private static string FormatReferenceFields(this NamedContentData data, Schema schema, string partitionKey, string separator)
        {
            Guard.NotNull(schema, nameof(schema));

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
