// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ReferencesExtensions
    {
        public static IEnumerable<Guid> ExtractReferences(this IField field, JToken value)
        {
            return ReferencesExtractor.ExtractReferences(field, value);
        }

        public static JToken CleanReferences(this IField field, JToken value, ICollection<Guid> oldReferences)
        {
            if (value.IsNull())
            {
                return value;
            }

            return ReferencesCleaner.CleanReferences(field, value, oldReferences);
        }

        public static JToken ToJToken(this HashSet<Guid> ids)
        {
            var result = new JArray();

            foreach (var id in ids)
            {
                result.Add(new JValue(id));
            }

            return result;
        }

        public static HashSet<Guid> ToGuidSet(this JToken value)
        {
            if (value is JArray ids)
            {
                var result = new HashSet<Guid>();

                foreach (var id in ids)
                {
                    if (id.Type == JTokenType.Guid)
                    {
                        result.Add((Guid)id);
                    }
                    else if (id.Type == JTokenType.String && Guid.TryParse((string)id, out var guid))
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
