// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.ExtractReferenceIds
{
    public static class ContentReferencesExtensions
    {
        public static IEnumerable<Guid> GetReferencedIds(this IdContentData source, Schema schema)
        {
            Guard.NotNull(schema, nameof(schema));

            var foundReferences = new HashSet<Guid>();

            foreach (var field in schema.Fields)
            {
                var fieldData = source.GetOrDefault(field.Id);

                if (fieldData == null)
                {
                    continue;
                }

                foreach (var partitionValue in fieldData.Where(x => !x.Value.IsNull()))
                {
                    var ids = field.ExtractReferences(partitionValue.Value);

                    foreach (var id in ids.Where(x => foundReferences.Add(x)))
                    {
                        yield return id;
                    }
                }
            }
        }
    }
}
