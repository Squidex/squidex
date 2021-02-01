// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndexerState : MongoRepositoryBase<TextContentState>, ITextIndexerState
    {
        static MongoTextIndexerState()
        {
            BsonClassMap.RegisterClassMap<TextContentState>(cm =>
            {
                cm.MapIdField(x => x.UniqueContentId);

                cm.MapProperty(x => x.DocIdCurrent)
                    .SetElementName("c");

                cm.MapProperty(x => x.DocIdNew)
                    .SetElementName("n").SetIgnoreIfNull(true);

                cm.MapProperty(x => x.DocIdForPublished)
                    .SetElementName("p").SetIgnoreIfNull(true);
            });
        }

        public MongoTextIndexerState(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
        }

        protected override string CollectionName()
        {
            return "TextIndexerState";
        }

        public async Task<Dictionary<DomainId, TextContentState>> GetAsync(HashSet<DomainId> ids)
        {
            var entities = await Collection.Find(Filter.In(x => x.UniqueContentId, ids)).ToListAsync();

            return entities.ToDictionary(x => x.UniqueContentId);
        }

        public Task SetAsync(List<TextContentState> updates)
        {
            var writes = new List<WriteModel<TextContentState>>();

            foreach (var update in updates)
            {
                if (update.IsDeleted)
                {
                    writes.Add(
                        new DeleteOneModel<TextContentState>(
                            Filter.Eq(x => x.UniqueContentId, update.UniqueContentId)));
                }
                else
                {
                    writes.Add(
                        new ReplaceOneModel<TextContentState>(
                            Filter.Eq(x => x.UniqueContentId, update.UniqueContentId), update)
                        {
                            IsUpsert = true
                        });
                }
            }

            if (writes.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Collection.BulkWriteAsync(writes);
        }
    }
}