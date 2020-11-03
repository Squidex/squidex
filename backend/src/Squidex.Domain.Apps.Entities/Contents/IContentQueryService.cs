// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentQueryService
    {
        Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, IReadOnlyList<DomainId> ids);

        Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, string schemaIdOrName, Q query);

        Task<IEnrichedContentEntity> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = EtagVersion.Any);

        Task<ISchemaEntity> GetSchemaOrThrowAsync(Context context, string schemaIdOrName);
    }
}
