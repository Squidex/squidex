// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.Tasks;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.MongoDb
{
    public abstract class MongoRepositoryBase<TEntity> : IInitializable
    {
        private const string CollectionFormat = "{0}Set";

        protected static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        protected static readonly SortDefinitionBuilder<TEntity> Sort = Builders<TEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<TEntity> Update = Builders<TEntity>.Update;
        protected static readonly FieldDefinitionBuilder<TEntity> Fields = FieldDefinitionBuilder<TEntity>.Instance;
        protected static readonly FilterDefinitionBuilder<TEntity> Filter = Builders<TEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<TEntity> Index = Builders<TEntity>.IndexKeys;
        protected static readonly ProjectionDefinitionBuilder<TEntity> Projection = Builders<TEntity>.Projection;

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
                return Task.Run(async () =>
                {
                    var databaseCollection = mongoDatabase.GetCollection<TEntity>(
                        CollectionName(),
                        CollectionSettings() ?? new MongoCollectionSettings());

                    await SetupCollectionAsync(databaseCollection).ConfigureAwait(false);

                    return databaseCollection;
                }).Result;
            });
        }

        protected virtual Task SetupCollectionAsync(IMongoCollection<TEntity> collection)
        {
            return TaskHelper.Done;
        }

        public virtual async Task ClearAsync()
        {
            await Database.DropCollectionAsync(CollectionName());

            await SetupCollectionAsync(Collection);
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

        public void Initialize()
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