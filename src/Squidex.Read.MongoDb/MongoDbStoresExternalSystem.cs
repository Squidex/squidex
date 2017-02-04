// ==========================================================================
//  MongoDbStoresExternalSystem.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Driver;
using Squidex.Infrastructure;

namespace Squidex.Read.MongoDb
{
    public sealed class MongoDbStoresExternalSystem : IExternalSystem
    {
        private readonly IMongoDatabase database;

        public MongoDbStoresExternalSystem(IMongoDatabase database)
        {
            Guard.NotNull(database, nameof(database));

            this.database = database;
        }
        
        public void CheckConnection()
        {
            try
            {
                database.ListCollections();
            }
            catch (Exception e)
            {
                throw new ConfigurationException($"MongoDb Event Store failed to connect to database {database.DatabaseNamespace.DatabaseName}", e);
            }
        }
    }
}
