// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class EnrichForCaching : IContentEnricherStep
    {
        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas)
        {
            var app = context.App;

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                var schema = await schemas(group.Key);

                foreach (var content in group)
                {
                    content.CacheDependencies ??= new HashSet<object?>();

                    content.CacheDependencies.Add(app.Id);
                    content.CacheDependencies.Add(app.Version);
                    content.CacheDependencies.Add(schema.Id);
                    content.CacheDependencies.Add(schema.Version);
                }
            }
        }
    }
}
