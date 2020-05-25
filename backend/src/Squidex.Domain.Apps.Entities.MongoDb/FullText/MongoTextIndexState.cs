// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndexState
    {
        [BsonId]
        [BsonElement]
        public DomainId DocumentId { get; set; }

        [BsonRequired]
        [BsonElement]
        public DomainId ContentId { get; set; }

        [BsonRequired]
        [BsonElement("c")]
        public string DocIdCurrent { get; set; }

        [BsonRequired]
        [BsonElement("n")]
        public string? DocIdNew { get; set; }

        [BsonRequired]
        [BsonElement("p")]
        public string? DocIdForPublished { get; set; }

        public MongoTextIndexState()
        {
        }

        public MongoTextIndexState(DomainId documentId, TextContentState state)
        {
            DocumentId = documentId;

            ContentId = state.ContentId;
            DocIdNew = state.DocIdNew;
            DocIdCurrent = state.DocIdCurrent;
            DocIdForPublished = state.DocIdForPublished;
        }

        public TextContentState ToState()
        {
            return new TextContentState
            {
                ContentId = ContentId,
                DocIdNew = DocIdNew,
                DocIdCurrent = DocIdCurrent,
                DocIdForPublished = DocIdForPublished
            };
        }
    }
}
