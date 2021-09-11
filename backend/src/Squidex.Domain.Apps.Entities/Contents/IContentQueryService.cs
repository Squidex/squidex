// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public interface IContentQueryService
    {
        Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, Q q,
            CancellationToken ct = default);

        Task<IResultList<IEnrichedContentEntity>> QueryAsync(Context context, string schemaIdOrName, Q query,
            CancellationToken ct = default);

        Task<IEnrichedContentEntity?> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = EtagVersion.Any,
            CancellationToken ct = default);

        Task<ISchemaEntity> GetSchemaOrThrowAsync(Context context, string schemaIdOrName,
            CancellationToken ct = default);

        Task<ISchemaEntity?> GetSchemaAsync(Context context, string schemaIdOrNama,
            CancellationToken ct = default);
    }
}
