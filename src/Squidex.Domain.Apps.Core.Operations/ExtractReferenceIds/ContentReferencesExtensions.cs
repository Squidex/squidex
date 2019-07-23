// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ContentReferencesExtensions
    {
        public static IEnumerable<Guid> GetReferencedIds(this IdContentData source, Schema schema, Ids strategy = Ids.All)
        {
            Guard.NotNull(schema, nameof(schema));

            foreach (var field in schema.Fields)
            {
                var ids = source.GetReferencedIds(field, strategy);

                foreach (var id in ids)
                {
                    yield return id;
                }
            }
        }

        public static IEnumerable<Guid> GetReferencedIds(this IdContentData source, IField field, Ids strategy = Ids.All)
        {
            Guard.NotNull(field, nameof(field));

            if (source.TryGetValue(field.Id, out var fieldData))
            {
                foreach (var partitionValue in fieldData)
                {
                    var ids = field.GetReferencedIds(partitionValue.Value, strategy);

                    foreach (var id in ids)
                    {
                        yield return id;
                    }
                }
            }
        }

        public static IEnumerable<Guid> GetReferencedIds(this NamedContentData source, Schema schema, Ids strategy = Ids.All)
        {
            Guard.NotNull(schema, nameof(schema));

            foreach (var field in schema.Fields)
            {
                var ids = source.GetReferencedIds(field, strategy);

                foreach (var id in ids)
                {
                    yield return id;
                }
            }
        }

        public static IEnumerable<Guid> GetReferencedIds(this NamedContentData source, IField field, Ids strategy = Ids.All)
        {
            Guard.NotNull(field, nameof(field));

            if (source.TryGetValue(field.Name, out var fieldData))
            {
                foreach (var partitionValue in fieldData)
                {
                    var ids = field.GetReferencedIds(partitionValue.Value, strategy);

                    foreach (var id in ids)
                    {
                        yield return id;
                    }
                }
            }
        }

        public static JsonObject FormatReferences(this NamedContentData data, Schema schema, LanguagesConfig languages, string separator = ", ")
        {
            Guard.NotNull(schema, nameof(schema));

            var result = JsonValue.Object();

            foreach (var language in languages)
            {
                result[language.Key] = JsonValue.Create(data.FormatReferenceFields(schema, language.Key, separator));
            }

            return result;
        }

        private static string FormatReferenceFields(this NamedContentData data, Schema schema, string partition, string separator)
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

            var referenceFields = schema.Fields.Where(x => x.RawProperties.IsReferenceField);

            if (!referenceFields.Any())
            {
                referenceFields = schema.Fields.Take(1);
            }

            foreach (var referenceField in referenceFields)
            {
                if (data.TryGetValue(referenceField.Name, out var fieldData))
                {
                    if (fieldData.TryGetValue(partition, out var value))
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
