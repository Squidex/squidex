// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, HashSet<DomainId> ids, SearchScope scope);

        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, HashSet<DomainId> ids, SearchScope scope);

        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, ClrQuery query, SearchScope scope);

        Task<IReadOnlyList<(DomainId SchemaId, DomainId Id)>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode);

        Task<IReadOnlyList<(DomainId SchemaId, DomainId Id)>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids, SearchScope scope);

        Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, DomainId id, SearchScope scope);

        Task ResetScheduledAsync(DomainId id);

        Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback);
    }
}
