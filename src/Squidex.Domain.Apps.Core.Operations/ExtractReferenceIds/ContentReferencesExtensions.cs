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
                var ids = source.GetReferencedIds(field);

                foreach (var id in ids)
                {
                    if (foundReferences.Add(id))
                    {
                        yield return id;
                    }
                }
            }
        }

        public static IEnumerable<Guid> GetReferencedIds(this IdContentData source, IField field)
        {
            Guard.NotNull(field, nameof(field));

            if (source.TryGetValue(field.Id, out var fieldData))
            {
                foreach (var partitionValue in fieldData)
                {
                    var ids = field.GetReferencedIds(partitionValue.Value);

                    foreach (var id in ids)
                    {
                        yield return id;
                    }
                }
            }
        }

        public static IEnumerable<Guid> GetReferencedIds(this NamedContentData source, IField field)
        {
            Guard.NotNull(field, nameof(field));

            if (source.TryGetValue(field.Name, out var fieldData))
            {
                foreach (var partitionValue in fieldData)
                {
                    var ids = field.GetReferencedIds(partitionValue.Value);

                    foreach (var id in ids)
                    {
                        yield return id;
                    }
                }
            }
        }
    }
}
