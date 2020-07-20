// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public delegate Task<ISchemaEntity> ProvideSchema(DomainId id);

    public interface IContentEnricherStep
    {
        Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas);

        Task EnrichAsync(Context context)
        {
            return Task.CompletedTask;
        }
    }
}
