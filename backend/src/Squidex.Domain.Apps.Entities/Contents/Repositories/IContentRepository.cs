﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.Repositories;

public interface IContentRepository
{
    IAsyncEnumerable<Content> StreamScheduledWithoutDataAsync(Instant now, SearchScope scope,
        CancellationToken ct = default);

    IAsyncEnumerable<DomainId> StreamIds(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default);

    IAsyncEnumerable<Content> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default);

    IAsyncEnumerable<Content> StreamReferencing(DomainId appId, DomainId references, int take, SearchScope scope,
        CancellationToken ct = default);

    Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, Q q, SearchScope scope,
        CancellationToken ct = default);

    Task<IResultList<Content>> QueryAsync(App app, Schema schema, Q q, SearchScope scope,
        CancellationToken ct = default);

    Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, Schema schema, FilterNode<ClrValue> filterNode, SearchScope scope,
        CancellationToken ct = default);

    Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, HashSet<DomainId> ids, SearchScope scope,
        CancellationToken ct = default);

    Task<Content?> FindContentAsync(App app, Schema schema, DomainId id, IReadOnlySet<string>? fields, SearchScope scope,
        CancellationToken ct = default);

    Task<bool> HasReferrersAsync(App app, DomainId reference, SearchScope scope,
        CancellationToken ct = default);

    Task ResetScheduledAsync(DomainId appId, DomainId id, SearchScope scope,
        CancellationToken ct = default);

    Task CreateIndexAsync(DomainId appId, DomainId schemaId, IndexDefinition index,
        CancellationToken ct = default);

    Task DropIndexAsync(DomainId appId, DomainId schemaId, string name,
        CancellationToken ct = default);

    Task<List<IndexDefinition>> GetIndexesAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct = default);
}
