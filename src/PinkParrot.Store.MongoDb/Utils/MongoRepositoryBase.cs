// ==========================================================================
//  MongoRepositoryBase.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Globalization;
using System.Threading.Tasks;
using MongoDB.Driver;
using PinkParrot.Infrastructure;

namespace PinkParrot.Store.MongoDb.Utils
{
    public abstract class MongoRepositoryBase<TEntity>
    {
        private const string CollectionFormat = "{0}Set";
        private readonly IMongoCollection<TEntity> mongoCollection;
        private readonly IMongoDatabase mongoDatabase;
        private readonly string typeName;

        protected string TypeName
        {
            get
            {
                return typeName;
            }
        }

        protected ProjectionDefinitionBuilder<TEntity> Projection
        {
            get
            {
                return Builders<TEntity>.Projection;
            }
        }

        protected SortDefinitionBuilder<TEntity> Sort
        {
            get
            {
                return Builders<TEntity>.Sort;
            }
        }

        protected UpdateDefinitionBuilder<TEntity> Update
        {
            get
            {
                return Builders<TEntity>.Update;
            }
        }

        protected FilterDefinitionBuilder<TEntity> Filter
        {
            get
            {
                return Builders<TEntity>.Filter;
            }
        }

        protected IndexKeysDefinitionBuilder<TEntity> IndexKeys
        {
            get
            {
                return Builders<TEntity>.IndexKeys;
            }
        }

        protected IMongoCollection<TEntity> Collection
        {
            get
            {
                return mongoCollection;
            }
        }

        protected IMongoDatabase Database
        {
            get
            {
                return mongoDatabase;
            }
        }

        protected MongoRepositoryBase(IMongoDatabase database)
        {
            Guard.NotNull(database, nameof(database));

            mongoDatabase = database;
            mongoCollection = CreateCollection();

            typeName = GetType().Name;
        }

        protected virtual MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings();
        }

        protected virtual string CollectionName()
        {
            return string.Format(CultureInfo.InvariantCulture, CollectionFormat, typeof(TEntity).Name);
        }

        private IMongoCollection<TEntity> CreateCollection()
        {
            var databaseCollection = mongoDatabase.GetCollection<TEntity>(
                CollectionName(),
                CollectionSettings() ?? new MongoCollectionSettings());

            SetupCollectionAsync(databaseCollection).Wait();

            return databaseCollection;
        }

        protected virtual Task SetupCollectionAsync(IMongoCollection<TEntity> collection)
        {
            return Task.FromResult(true);
        }
    }
}