// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndexerState : MongoRepositoryBase<MongoTextIndexState>, ITextIndexerState
    {
        public MongoTextIndexerState(IMongoDatabase database, bool setup = false)
            : base(database, setup)
        {
        }

        protected override string CollectionName()
        {
            return "TextIndexerState";
        }

        public async Task<TextContentState?> GetAsync(DomainId appId, DomainId contentId)
        {
            var documentId = DomainId.Combine(appId, contentId).ToString();

            var result = await Collection.Find(x => x.DocumentId == documentId).FirstOrDefaultAsync()!;

            return result?.ToState();
        }

        public Task RemoveAsync(DomainId appId, DomainId contentId)
        {
            var documentId = DomainId.Combine(appId, contentId).ToString();

            return Collection.DeleteOneAsync(x => x.DocumentId == documentId);
        }

        public Task SetAsync(DomainId appId, TextContentState state)
        {
            var documentId = DomainId.Combine(appId, state.ContentId).ToString();
            var document = new MongoTextIndexState(documentId, state);

            return Collection.ReplaceOneAsync(x => x.DocumentId == documentId, document, UpsertReplace);
        }
    }
}
