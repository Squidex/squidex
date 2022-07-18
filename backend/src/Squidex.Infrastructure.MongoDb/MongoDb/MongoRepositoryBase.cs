// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Hosting;
using Squidex.Hosting.Configuration;

namespace Squidex.Infrastructure.MongoDb
{
    public abstract class MongoRepositoryBase<TEntity> : MongoBase<TEntity>, IInitializable
    {
        private readonly IMongoDatabase mongoDatabase;
        private IMongoCollection<TEntity> mongoCollection;

        protected IMongoCollection<TEntity> Collection
        {
            get
            {
                if (mongoCollection == null)
                {
                    ThrowHelper.InvalidOperationException("Collection has not been initialized yet.");
                    return default!;
                }

                return mongoCollection;
            }
        }

        protected IMongoDatabase Database
        {
            get => mongoDatabase;
        }

        protected MongoRepositoryBase(IMongoDatabase database, bool setup = false)
        {
            Guard.NotNull(database);

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

        protected abstract string CollectionName();

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
