﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ReferencesExtensions
    {
        public static IEnumerable<Guid> GetReferencedIds(this IField field, IJsonValue? value, Ids strategy = Ids.All)
        {
            return ReferencesExtractor.ExtractReferences(field, value, strategy);
        }

        public static IJsonValue CleanReferences(this IField field, IJsonValue value, ICollection<Guid>? oldReferences)
        {
            if (IsNull(value))
            {
                return value;
            }

            return ReferencesCleaner.CleanReferences(field, value, oldReferences);
        }

        private static bool IsNull(IJsonValue value)
        {
            return value == null || value.Type == JsonValueType.Null;
        }

        public static JsonArray ToJsonArray(this HashSet<Guid> ids)
        {
            var result = JsonValue.Array();

            foreach (var id in ids)
            {
                result.Add(JsonValue.Create(id.ToString()));
            }

            return result;
        }

        public static HashSet<Guid> ToGuidSet(this IJsonValue? value)
        {
            if (value is JsonArray array)
            {
                var result = new HashSet<Guid>();

                foreach (var id in array)
                {
                    if (id.Type == JsonValueType.String && Guid.TryParse(id.ToString(), out var guid))
                    {
                        result.Add(guid);
                    }
                }

                return result;
            }

            return new HashSet<Guid>();
        }
    }
}
