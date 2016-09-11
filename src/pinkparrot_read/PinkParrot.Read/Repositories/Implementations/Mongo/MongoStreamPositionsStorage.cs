// ==========================================================================
//  MongoStreamPositionsStorage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using PinkParrot.Infrastructure.CQRS.EventStore;
using PinkParrot.Infrastructure.MongoDb;

// ReSharper disable InvertIf

namespace PinkParrot.Read.Repositories.Implementations.Mongo
{
    public sealed class MongoStreamPositionsStorage : MongoRepositoryBase<MongoPosition>, IStreamPositionStorage
    {
        private static readonly ObjectId Id = new ObjectId("507f1f77bcf86cd799439011");

        public MongoStreamPositionsStorage(IMongoDatabase database)
            : base(database)
        {
        }

        public int? ReadPosition()
        {
            var document = Collection.Find(t => t.Id == Id).FirstOrDefault();

            if (document == null)
            {
                document = new MongoPosition { Id = Id };

                Collection.InsertOne(document);
            }

            return document.Position;
        }

        public void WritePosition(int position)
        {
            Collection.UpdateOne(t => t.Id == Id, Update.Set(t => t.Position, position));
        }
    }
}