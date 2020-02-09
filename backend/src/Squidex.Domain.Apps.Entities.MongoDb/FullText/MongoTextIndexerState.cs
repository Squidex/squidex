// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndexerState : MongoRepositoryBase<TextContentState>, ITextIndexerState
    {
        static MongoTextIndexerState()
        {
            BsonClassMap.RegisterClassMap<TextContentState>(cm =>
            {
                cm.MapIdField(x => x.ContentId);

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

        public Task<TextContentState?> GetAsync(Guid contentId)
        {
            return Collection.Find(x => x.ContentId == contentId).FirstOrDefaultAsync()!;
        }

        public Task RemoveAsync(Guid contentId)
        {
            return Collection.DeleteOneAsync(x => x.ContentId == contentId);
        }

        public Task SetAsync(TextContentState state)
        {
            return Collection.ReplaceOneAsync(x => x.ContentId == state.ContentId, state, UpsertReplace);
        }
    }
}
