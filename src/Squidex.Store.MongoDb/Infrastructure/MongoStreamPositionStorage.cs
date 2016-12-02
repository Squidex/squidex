// ==========================================================================
//  MongoStreamPositionStorage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.EventStore;
using Squidex.Store.MongoDb.Utils;

// ReSharper disable InvertIf

namespace Squidex.Store.MongoDb.Infrastructure
{
    public sealed class MongoStreamPositionStorage : MongoRepositoryBase<MongoStreamPositionEntity>, IStreamPositionStorage
    {
        public MongoStreamPositionStorage(IMongoDatabase database)
            : base(database)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoStreamPositionEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.SubscriptionName), new CreateIndexOptions { Unique = true });
        }

        protected override string CollectionName()
        {
            return "StreamPositions";
        }

        public int? ReadPosition(string subscriptionName)
        {
            Guard.NotNullOrEmpty(subscriptionName, nameof(subscriptionName));

            var document = Collection.Find(t => t.SubscriptionName == subscriptionName).FirstOrDefault();

            if (document == null)
            {
                document = new MongoStreamPositionEntity { SubscriptionName = subscriptionName };

                Collection.InsertOne(document);
            }

            return document.Position;
        }

        public void WritePosition(string subscriptionName, int position)
        {
            Guard.NotNullOrEmpty(subscriptionName, nameof(subscriptionName));

            Collection.UpdateOne(t => t.SubscriptionName == subscriptionName, Update.Set(t => t.Position, position));
        }
    }
}