// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class MongoTextIndexFixture
    {
        public MongoTextIndex Index { get; }

        public MongoTextIndexFixture()
        {
            BsonJsonConvention.Register(JsonSerializer.Create(TestUtils.CreateSerializerSettings()));

            DomainIdSerializer.Register();

            var mongoClient = new MongoClient(TestConfig.Configuration["mongodb:configuration"]);
            var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

            Index = new MongoTextIndex(mongoDatabase, false);
            Index.InitializeAsync(default).Wait();
        }
    }
}
