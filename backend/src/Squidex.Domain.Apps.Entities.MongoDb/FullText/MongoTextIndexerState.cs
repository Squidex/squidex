// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

        public Task<TextContentState?> GetAsync(DomainId appId, DomainId contentId)
        {
            var documentId = DomainId.Combine(appId, contentId).ToString();

            return Collection.Find(x => x.ContentId == contentId).FirstOrDefaultAsync()!;
        }

        public Task RemoveAsync(DomainId appId, DomainId contentId)
        {
            var documentId = DomainId.Combine(appId, contentId).ToString();

            return Collection.DeleteOneAsync(x => x.ContentId == contentId);
        }

        public Task SetAsync(DomainId appId, TextContentState state)
        {
            var documentId = DomainId.Combine(appId, state.ContentId).ToString();

            return Collection.ReplaceOneAsync(x => x.ContentId == state.ContentId, state, UpsertReplace);
        }
    }
}
