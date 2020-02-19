// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class EnrichWithSchema : IContentEnricherStep
    {
        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await schemas(group.Key);

                var schemaName = schema.SchemaDef.Name;
                var schemaDisplayName = schema.SchemaDef.DisplayNameUnchanged();

                foreach (var content in group)
                {
                    content.IsSingleton = schema.SchemaDef.IsSingleton;

                    content.SchemaName = schemaName;
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
