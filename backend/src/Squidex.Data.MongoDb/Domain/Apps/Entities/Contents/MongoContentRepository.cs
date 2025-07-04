﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public partial class MongoContentRepository(
    IMongoDatabase database,
    IAppProvider appProvider,
    string shardKey,
    IOptions<ContentsOptions> options,
    ILogger<MongoContentRepository> log)
    : MongoBase<MongoContentEntity>, IContentRepository, IInitializable
{
    private readonly MongoContentCollection collectionComplete =
        new MongoContentCollection(
            $"States_Contents_All3{shardKey}",
            database,
            log,
            ReadPreference.Primary,
            options.Value.OptimizeForSelfHosting);
    private readonly MongoContentCollection collectionPublished =
        new MongoContentCollection(
            $"States_Contents_Published3{shardKey}",
            database,
            log,
            ReadPreference.Secondary,
            options.Value.OptimizeForSelfHosting);
    private readonly ContentsOptions options = options.Value;

    public bool CanUseTransactions { get; private set; }

    static MongoContentRepository()
    {
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonEscapedDictionarySerializer<JsonValue, ContentFieldData>.Register();
        BsonEscapedDictionarySerializer<ContentFieldData, ContentData>.Register();
        BsonStringSerializer<Status>.Register();
        MongoContentEntity.RegisterClassMap();
    }

    public async Task InitializeAsync(
        CancellationToken ct)
    {
        await collectionComplete.InitializeAsync(ct);
        await collectionPublished.InitializeAsync(ct);

        var clusterVersion = await database.GetMajorVersionAsync(ct);
        var clusteredAsReplica = database.Client.Cluster.Description.Type == ClusterType.ReplicaSet;

        CanUseTransactions = clusteredAsReplica && clusterVersion >= 4 && options.UseTransactions;
    }

    public IAsyncEnumerable<Content> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).StreamAll(appId, schemaIds, ct);
    }

    public IAsyncEnumerable<Content> StreamReferencing(DomainId appId, DomainId reference, int take, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).StreamReferencing(appId, reference, take, ct);
    }

    public IAsyncEnumerable<Content> StreamScheduledWithoutDataAsync(Instant now, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryScheduledWithoutDataAsync(now, ct);
    }

    public IAsyncEnumerable<DomainId> StreamIds(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).StreamIds(appId, schemaIds, ct);
    }

    public Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryAsync(app, schemas, q, ct);
    }

    public Task<IResultList<Content>> QueryAsync(App app, Schema schema, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryAsync(app, schema, q, ct);
    }

    public Task<Content?> FindContentAsync(App app, Schema schema, DomainId id, IReadOnlySet<string>? fields, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).FindContentAsync(schema, id, fields, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, HashSet<DomainId> ids, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryIdsAsync(app, ids, ct);
    }

    public Task<bool> HasReferrersAsync(App app, DomainId reference, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).HasReferrersAsync(app, reference, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, Schema schema, FilterNode<ClrValue> filterNode, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryIdsAsync(app, schema, filterNode, ct);
    }

    public Task ResetScheduledAsync(DomainId appId, DomainId id, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(SearchScope.All).ResetScheduledAsync(appId, id, ct);
    }

    public Task CreateIndexAsync(DomainId appId, DomainId schemaId, IndexDefinition index,
        CancellationToken ct = default)
    {
        return GetCollection(SearchScope.All).CreateIndexAsync(appId, schemaId, index, ct);
    }

    public Task<List<IndexDefinition>> GetIndexesAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct = default)
    {
        return GetCollection(SearchScope.All).GetIndexesAsync(appId, schemaId, ct);
    }

    public Task DropIndexAsync(DomainId appId, DomainId schemaId, string name,
        CancellationToken ct = default)
    {
        return GetCollection(SearchScope.All).DropIndexAsync(appId, schemaId, name, ct);
    }

    private MongoContentCollection GetCollection(SearchScope scope)
    {
        return scope == SearchScope.All ? collectionComplete : collectionPublished;
    }
}
