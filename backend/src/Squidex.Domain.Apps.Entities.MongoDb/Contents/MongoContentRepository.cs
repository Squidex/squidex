// ==========================================================================
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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public partial class MongoContentRepository : MongoBase<MongoContentEntity>, IContentRepository, IInitializable
{
    private readonly MongoContentCollection collectionComplete;
    private readonly MongoContentCollection collectionPublished;
    private readonly ContentOptions options;
    private readonly IMongoDatabase database;
    private readonly IAppProvider appProvider;

    public bool CanUseTransactions { get; private set; }

    static MongoContentRepository()
    {
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonEscapedDictionarySerializer<JsonValue, ContentFieldData>.Register();
        BsonEscapedDictionarySerializer<ContentFieldData, ContentData>.Register();
        BsonStringSerializer<Status>.Register();
    }

    public MongoContentRepository(IMongoDatabase database, IAppProvider appProvider,
        IOptions<ContentOptions> options, ILogger<MongoContentRepository> log)
    {
        this.appProvider = appProvider;
        this.database = database;
        this.options = options.Value;

        collectionComplete =
            new MongoContentCollection("States_Contents_All3", database, log,
                ReadPreference.Primary, options.Value.OptimizeForSelfHosting);

        collectionPublished =
            new MongoContentCollection("States_Contents_Published3", database, log,
                ReadPreference.Secondary, options.Value.OptimizeForSelfHosting);
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

    public IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).StreamAll(appId, schemaIds, ct);
    }

    public IAsyncEnumerable<IContentEntity> StreamReferencing(DomainId appId, DomainId reference, int take, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).StreamReferencing(appId, reference, take, ct);
    }

    public IAsyncEnumerable<IContentEntity> StreamScheduledWithoutDataAsync(Instant now, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryScheduledWithoutDataAsync(now, ct);
    }

    public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryAsync(app, schemas, q, ct);
    }

    public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryAsync(app, schema, q, ct);
    }

    public Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, DomainId id, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).FindContentAsync(schema, id, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryIdsAsync(appId, ids, ct);
    }

    public Task<bool> HasReferrersAsync(DomainId appId, DomainId reference, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).HasReferrersAsync(appId, reference, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(scope).QueryIdsAsync(appId, schemaId, filterNode, ct);
    }

    public Task ResetScheduledAsync(DomainId documentId, SearchScope scope,
        CancellationToken ct = default)
    {
        return GetCollection(SearchScope.All).ResetScheduledAsync(documentId, ct);
    }

    private MongoContentCollection GetCollection(SearchScope scope)
    {
        return scope == SearchScope.All ? collectionComplete : collectionPublished;
    }
}
