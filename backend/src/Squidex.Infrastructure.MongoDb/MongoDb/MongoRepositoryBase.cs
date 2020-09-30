// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.MongoDb
{
    public abstract class MongoRepositoryBase<TEntity> : IInitializable
    {
        private const string CollectionFormat = "{0}Set";

        protected static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        protected static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };
        protected static readonly SortDefinitionBuilder<TEntity> Sort = Builders<TEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<TEntity> Update = Builders<TEntity>.Update;
        protected static readonly FieldDefinitionBuilder<TEntity> FieldBuilder = FieldDefinitionBuilder<TEntity>.Instance;
        protected static readonly FilterDefinitionBuilder<TEntity> Filter = Builders<TEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<TEntity> Index = Builders<TEntity>.IndexKeys;
        protected static readonly ProjectionDefinitionBuilder<TEntity> Projection = Builders<TEntity>.Projection;

        private readonly IMongoDatabase mongoDatabase;
        private IMongoCollection<TEntity> mongoCollection;

        protected IMongoCollection<TEntity> Collection
        {
            get
            {
                if (mongoCollection == null)
                {
                    throw new InvalidOperationException("Collection has not been initialized yet.");
                }

                return mongoCollection;
            }
        }

        protected IMongoDatabase Database
        {
            get { return mongoDatabase; }
        }

        static MongoRepositoryBase()
        {
            RefTokenSerializer.Register();

            InstantSerializer.Register();

            DomainIdSerializer.Register();
        }

        protected MongoRepositoryBase(IMongoDatabase database, bool setup = false)
        {
            Guard.NotNull(database, nameof(database));

            mongoDatabase = database;

            if (setup)
            {
                CreateCollection();
            }
        }

        protected virtual MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings();
        }

        protected virtual string CollectionName()
        {
            return string.Format(CultureInfo.InvariantCulture, CollectionFormat, typeof(TEntity).Name);
        }

        protected virtual Task SetupCollectionAsync(IMongoCollection<TEntity> collection, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public virtual async Task ClearAsync()
        {
            try
            {
                await Database.DropCollectionAsync(CollectionName());
            }
            catch (MongoCommandException ex)
            {
                if (ex.Code != 26)
                {
                    throw;
                }
            }

            await InitializeAsync();
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                CreateCollection();

                await SetupCollectionAsync(Collection, ct);
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"MongoDb connection failed to connect to database {Database.DatabaseNamespace.DatabaseName}", ex);
            }
        }

        private void CreateCollection()
        {
            mongoCollection = mongoDatabase.GetCollection<TEntity>(
                CollectionName(),
                CollectionSettings() ?? new MongoCollectionSettings());
        }
    }
}