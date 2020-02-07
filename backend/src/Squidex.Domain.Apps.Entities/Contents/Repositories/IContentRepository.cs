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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Contents.Repositories
{
    public interface IContentRepository
    {
        Task<List<(IContentEntity Content, ISchemaEntity Schema)>> QueryAsync(IAppEntity app, Status[]? status, HashSet<Guid> ids, SearchScope scope);

        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, HashSet<Guid> ids, SearchScope scope);

        Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, ClrQuery query, SearchScope scope);

        Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, Guid schemaId, FilterNode<ClrValue> filterNode);

        Task<IReadOnlyList<(Guid SchemaId, Guid Id)>> QueryIdsAsync(Guid appId, HashSet<Guid> ids, SearchScope scope);

        Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, Status[]? status, Guid id, SearchScope scope);

        Task QueryScheduledWithoutDataAsync(Instant now, Func<IContentEntity, Task> callback);
    }
}
