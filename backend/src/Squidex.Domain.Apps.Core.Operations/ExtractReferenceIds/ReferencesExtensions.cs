// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ReferencesExtensions
    {
        public static void AddIds(this IJsonValue? value, HashSet<DomainId> result, int take)
        {
            var added = 0;

            if (value is JsonArray array)
            {
                foreach (var id in array)
                {
                    if (id is JsonString s)
                    {
                        result.Add(DomainId.Create(s.Value));

                        added++;

                        if (added >= take)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
