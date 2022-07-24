// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.MongoDb;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class MongoTextIndexFixture : IAsyncLifetime
    {
        public MongoTextIndex Index { get; }

        public MongoTextIndexFixture()
        {
            BsonJsonConvention.Register(TestUtils.DefaultOptions());

            BsonDomainIdSerializer.Register();

            var mongoClient = new MongoClient(TestConfig.Configuration["mongodb:configuration"]);
            var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

            Index = new MongoTextIndex(mongoDatabase);
        }

        public Task InitializeAsync()
        {
            return Index.InitializeAsync(default);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
