// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Hosting;
using Squidex.Hosting.Configuration;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.MongoDb
{
    public abstract class MongoRepositoryBase<TEntity> : IInitializable
    {
        private const string CollectionFormat = "{0}Set";

        protected static readonly BulkWriteOptions BulkUnordered = new BulkWriteOptions { IsOrdered = true };
        protected static readonly FieldDefinitionBuilder<TEntity> FieldBuilder = FieldDefinitionBuilder<TEntity>.Instance;
        protected static readonly FilterDefinitionBuilder<TEntity> Filter = Builders<TEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<TEntity> Index = Builders<TEntity>.IndexKeys;
        protected static readonly InsertManyOptions InsertUnordered = new InsertManyOptions { IsOrdered = true };
        protected static readonly ProjectionDefinitionBuilder<TEntity> Projection = Builders<TEntity>.Projection;
        protected static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };
        protected static readonly SortDefinitionBuilder<TEntity> Sort = Builders<TEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<TEntity> Update = Builders<TEntity>.Update;
        protected static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };

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
            get => mongoDatabase;
        }

        static MongoRepositoryBase()
        {
            TypeConverterStringSerializer<RefToken>.Register();

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

        protected virtual Task SetupCollectionAsync(IMongoCollection<TEntity> collection,
            CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public virtual async Task ClearAsync(
            CancellationToken ct = default)
        {
            try
            {
                await Database.DropCollectionAsync(CollectionName(), ct);
            }
            catch (MongoCommandException ex)
            {
                if (ex.Code != 26)
                {
                    throw;
                }
            }

            await InitializeAsync(ct);
        }

        public async Task InitializeAsync(
            CancellationToken ct)
        {
            try
            {
                CreateCollection();

                await SetupCollectionAsync(Collection, ct);
            }
            catch (Exception ex)
            {
                var databaseName = Database.DatabaseNamespace.DatabaseName;

                var error = new ConfigurationError($"MongoDb connection failed to connect to database {databaseName}.");

                throw new ConfigurationException(error, ex);
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
