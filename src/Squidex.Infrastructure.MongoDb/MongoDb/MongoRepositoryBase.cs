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

        protected static readonly SortDefinitionBuilder<TEntity> Sort = Builders<TEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<TEntity> Update = Builders<TEntity>.Update;
        protected static readonly FieldDefinitionBuilder<TEntity> Fields = FieldDefinitionBuilder<TEntity>.Instance;
        protected static readonly FilterDefinitionBuilder<TEntity> Filter = Builders<TEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<TEntity> Index = Builders<TEntity>.IndexKeys;
        protected static readonly ProjectionDefinitionBuilder<TEntity> Project = Builders<TEntity>.Projection;

        private readonly IMongoDatabase mongoDatabase;
        private Lazy<IMongoCollection<TEntity>> mongoCollection;

        protected IMongoCollection<TEntity> Collection
        {
            get { return mongoCollection.Value; }
        }

        protected IMongoDatabase Database
        {
            get { return mongoDatabase; }
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