// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndex : MongoRepositoryBase<MongoTextIndexEntity>, ITextIndex
    {
        private static readonly List<DomainId> EmptyResults = new List<DomainId>();

        public MongoTextIndex(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoTextIndexEntity> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoTextIndexEntity>(
                    Index
                        .Text("t.t")
                        .Ascending(x => x.AppId)
                        .Ascending(x => x.ServeAll)
                        .Ascending(x => x.ServePublished)
                        .Ascending(x => x.SchemaId))
            }, ct);
        }

        protected override string CollectionName()
        {
            return "TextIndex";
        }

        public Task ExecuteAsync(params IndexCommand[] commands)
        {
            var writes = new List<WriteModel<MongoTextIndexEntity>>(commands.Length);

            foreach (var command in commands)
            {
                switch (command)
                {
                    case DeleteIndexEntry _:
                        writes.Add(
                            new DeleteOneModel<MongoTextIndexEntity>(
                                Filter.Eq(x => x.DocId, command.DocId)));
                        break;
                    case UpdateIndexEntry update:
                        writes.Add(
                            new UpdateOneModel<MongoTextIndexEntity>(
                                Filter.Eq(x => x.DocId, command.DocId),
                                Update
                                    .Set(x => x.ServeAll, update.ServeAll)
                                    .Set(x => x.ServePublished, update.ServePublished)));
                        break;
                    case UpsertIndexEntry upsert when upsert.Texts.Count > 0:
                        writes.Add(
                            new ReplaceOneModel<MongoTextIndexEntity>(
                                Filter.Eq(x => x.DocId, command.DocId),
                                new MongoTextIndexEntity
                                {
                                    DocId = upsert.DocId,
                                    ContentId = upsert.ContentId,
                                    SchemaId = upsert.SchemaId.Id,
                                    ServeAll = upsert.ServeAll,
                                    ServePublished = upsert.ServePublished,
                                    Texts = upsert.Texts.Select(x => new MongoTextIndexEntityText { Text = x.Value }).ToList(),
                                    AppId = upsert.AppId.Id
                                })
                            {
                                IsUpsert = true
                            });
                        break;
                }
            }

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes);
        }

        public async Task<List<DomainId>?> SearchAsync(string? queryText, IAppEntity app, SearchFilter? filter, SearchScope scope)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return EmptyResults;
            }

            if (filter == null)
            {
                return await SearchByAppAsync(queryText, app, scope, 2000);
            }
            else if (filter.Must)
            {
                return await SearchBySchemaAsync(queryText, app, filter, scope, 2000);
            }
            else
            {
                var (bySchema, byApp) =
                    await AsyncHelper.WhenAll(
                        SearchBySchemaAsync(queryText, app, filter, scope, 1000),
                        SearchByAppAsync(queryText, app, scope, 1000));

                return bySchema.Union(byApp).Distinct().ToList();
            }
        }

        private async Task<List<DomainId>> SearchBySchemaAsync(string queryText, IAppEntity app, SearchFilter filter, SearchScope scope, int limit)
        {
            var bySchema =
                await Collection.Find(
                    Filter.And(
                        Filter.Eq(x => x.AppId, app.Id),
                        Filter.In(x => x.SchemaId, filter.SchemaIds),
                        Filter_ByScope(scope),
                        Filter.Text(queryText, "none")))
                    .Only(x => x.ContentId).Limit(limit)
                    .ToListAsync();

            return bySchema.Select(x => DomainId.Create(x["_ci"].AsString)).Distinct().ToList();
        }

        private async Task<List<DomainId>> SearchByAppAsync(string queryText, IAppEntity app, SearchScope scope, int limit)
        {
            var bySchema =
                await Collection.Find(
                    Filter.And(
                        Filter.Eq(x => x.AppId, app.Id),
                        Filter.Exists(x => x.SchemaId),
                        Filter_ByScope(scope),
                        Filter.Text(queryText, "none")))
                    .Only(x => x.ContentId).Limit(limit)
                    .ToListAsync();

            return bySchema.Select(x => DomainId.Create(x["_ci"].AsString)).Distinct().ToList();
        }

        private static FilterDefinition<MongoTextIndexEntity> Filter_ByScope(SearchScope scope)
        {
            if (scope == SearchScope.All)
            {
                return Filter.Eq(x => x.ServeAll, true);
            }
            else
            {
                return Filter.Eq(x => x.ServePublished, true);
            }
        }
    }
}
