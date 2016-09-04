// ==========================================================================
//  MongoStreamPositionsStorage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using EventStore.ClientAPI;
using MongoDB.Bson;
using MongoDB.Driver;
using PinkParrot.Infrastructure.CQRS.EventStore;
using PinkParrot.Infrastructure.MongoDb;

//// ReSharper disable once ConvertIfStatementToNullCoalescingExpression

namespace PinkParrot.Read.Services.Implementations
{
    public sealed class MongoStreamPositionsStorage : MongoRepositoryBase<MongoPosition>, IStreamPositionStorage
    {
        private static readonly ObjectId Id = new ObjectId("507f1f77bcf86cd799439011");

        public MongoStreamPositionsStorage(IMongoDatabase database)
            : base(database)
        {
        }

        public Position? ReadPosition()
        {
            var document = Collection.Find(t => t.Id == Id).FirstOrDefault<MongoPosition, MongoPosition>();

            return document != null ? new Position(document.CommitPosition, document.PreparePosition) : Position.Start;
        }

        public void WritePosition(Position position)
        {
            var document = Collection.Find(t => t.Id == Id).FirstOrDefault<MongoPosition, MongoPosition>();

            var isFound = document != null;

            if (document == null)
            {
                document = new MongoPosition { Id = Id };
            }

            document.CommitPosition = position.CommitPosition;
            document.PreparePosition = position.PreparePosition;

            if (isFound)
            {
                Collection.ReplaceOne(t => t.Id == Id, document);
            }
            else
            {
                Collection.InsertOne(document);
            }
        }
    }
}