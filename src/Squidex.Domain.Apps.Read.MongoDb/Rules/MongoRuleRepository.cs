// ==========================================================================
//  MongoRuleRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Rules
{
    public partial class MongoRuleRepository : MongoRepositoryBase<MongoRuleEntity>, IRuleRepository, IEventConsumer
    {
        private static readonly List<IRuleEntity> EmptyRules = new List<IRuleEntity>();
        private readonly SemaphoreSlim lockObject = new SemaphoreSlim(1);
        private Dictionary<Guid, List<IRuleEntity>> inMemoryRules;

        public MongoRuleRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_Rules";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoRuleEntity> collection)
        {
            return Task.WhenAll(collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId)));
        }

        public async Task<IReadOnlyList<IRuleEntity>> QueryByAppAsync(Guid appId)
        {
            var entities =
                await Collection.Find(x => x.AppId == appId)
                    .ToListAsync();

            return entities.OfType<IRuleEntity>().ToList();
        }

        public async Task<IReadOnlyList<IRuleEntity>> QueryCachedByAppAsync(Guid appId)
        {
            await EnsureRulesLoadedAsync();

            return inMemoryRules.GetOrDefault(appId) ?? EmptyRules;
        }

        private async Task EnsureRulesLoadedAsync()
        {
            if (inMemoryRules == null)
            {
                try
                {
                    await lockObject.WaitAsync();

                    if (inMemoryRules == null)
                    {
                        inMemoryRules = new Dictionary<Guid, List<IRuleEntity>>();

                        var webhooks =
                            await Collection.Find(new BsonDocument())
                                .ToListAsync();

                        foreach (var webhook in webhooks)
                        {
                            inMemoryRules.GetOrAddNew(webhook.AppId).Add(webhook);
                        }
                    }
                }
                finally
                {
                    lockObject.Release();
                }
            }
        }
    }
}
