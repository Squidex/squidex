// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Text;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class AtlasTextIndexFixture
    {
        public AtlasTextIndex Index { get; }

        public AtlasTextIndexFixture()
        {
            BsonJsonConvention.Register(JsonSerializer.Create(TestUtils.CreateSerializerSettings()));

            DomainIdSerializer.Register();

            var mongoClient = new MongoClient(TestConfig.Configuration["atlas:configuration"]);
            var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["atlas:database"]);

            var options = TestConfig.Configuration.GetSection("atlas").Get<AtlasOptions>();

            Index = new AtlasTextIndex(mongoDatabase, Options.Create(options), false);
            Index.InitializeAsync(default).Wait();
        }
    }
}
