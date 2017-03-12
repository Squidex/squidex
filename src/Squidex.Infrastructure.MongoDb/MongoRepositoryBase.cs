// ==========================================================================
//  MongoRepositoryBase.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.MongoDb
{
    public abstract class MongoRepositoryBase<TEntity> : IExternalSystem
    {
        private const string CollectionFormat = "{0}Set";
        private Lazy<IMongoCollection<TEntity>> mongoCollection;
        private readonly IMongoDatabase mongoDatabase;
        private readonly string typeName;

        protected string TypeName
        {
            get
            {
                return typeName;
            }
        }

        protected static ProjectionDefinitionBuilder<TEntity> Projection
        {
            get
            {
                return Builders<TEntity>.Projection;
            }
        }

        protected static SortDefinitionBuilder<TEntity> Sort
        {
            get
            {
                return Builders<TEntity>.Sort;
            }
        }

        protected static UpdateDefinitionBuilder<TEntity> Update
        {
            get
            {
                return Builders<TEntity>.Update;
            }
        }

        protected static FilterDefinitionBuilder<TEntity> Filter
        {
            get
            {
                return Builders<TEntity>.Filter;
            }
        }

        protected static IndexKeysDefinitionBuilder<TEntity> IndexKeys
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
                return mongoCollection.Value;
            }
        }

        protected IMongoDatabase Database
        {
            get
            {
                return mongoDatabase;
            }
        }

        static MongoRepositoryBase()
        {
            RefTokenSerializer.Register();
            InstantSerializer.Register();
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

        private Lazy<IMongoCollection<TEntity>> CreateCollection()
        {
            return new Lazy<IMongoCollection<TEntity>>(() =>
            {
                var databaseCollection = mongoDatabase.GetCollection<TEntity>(
                    CollectionName(),
                    CollectionSettings() ?? new MongoCollectionSettings());

                SetupCollectionAsync(databaseCollection).Wait();

                return databaseCollection;
            });
        }

        protected virtual Task SetupCollectionAsync(IMongoCollection<TEntity> collection)
        {
            return TaskHelper.Done;
        }

        public virtual Task ClearAsync()
        {
            return Collection.DeleteManyAsync(new BsonDocument());
        }

        public async Task<bool> DropCollectionIfExistsAsync()
        {
            try
            {
                await mongoDatabase.DropCollectionAsync(CollectionName());

                mongoCollection = CreateCollection();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Connect()
        {
            try
            {
                Database.ListCollections();
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"MongoDb connection failed to connect to database {Database.DatabaseNamespace.DatabaseName}", ex);
            }
        }
    }
}