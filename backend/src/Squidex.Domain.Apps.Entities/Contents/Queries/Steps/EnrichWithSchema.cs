// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class EnrichWithSchema : IContentEnricherStep
    {
        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                ct.ThrowIfCancellationRequested();

                var (schema, _) = await schemas(group.Key);

                var schemaDisplayName = schema.SchemaDef.DisplayNameUnchanged();

                foreach (var content in group)
                {
                    content.IsSingleton = schema.SchemaDef.Type == SchemaType.Singleton;

                    content.SchemaDisplayName = schemaDisplayName;
                }

                if (context.IsFrontendClient)
                {
                    var referenceFields = schema.SchemaDef.ReferenceFields().ToArray();

                    foreach (var content in group)
                    {
                        content.ReferenceFields = referenceFields;
                    }
                }
            }
        }
    }
}
