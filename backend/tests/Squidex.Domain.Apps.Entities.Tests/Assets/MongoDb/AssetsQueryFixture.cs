// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb
{
    public sealed class AssetsQueryFixture
    {
        private readonly Random random = new Random();
        private readonly int numValues = 250;
        private readonly IMongoClient mongoClient = new MongoClient("mongodb://localhost");
        private readonly IMongoDatabase mongoDatabase;

        public MongoAssetRepository AssetRepository { get; }

        public NamedId<DomainId>[] AppIds { get; } =
        {
            NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
            NamedId.Of(DomainId.Create("4b3672c1-97c6-4e0b-a067-71e9e9a29db9"), "my-app1")
        };

        public AssetsQueryFixture()
        {
            mongoDatabase = mongoClient.GetDatabase("Squidex_Testing");

            SetupJson();

            var assetRepository = new MongoAssetRepository(mongoDatabase);

            Task.Run(async () =>
            {
                await assetRepository.InitializeAsync(default);

                await mongoDatabase.RunCommandAsync<BsonDocument>("{ profile : 0 }");
                await mongoDatabase.DropCollectionAsync("system.profile");

                var collection = assetRepository.GetInternalCollection();

                var assetCount = await collection.Find(new BsonDocument()).CountDocumentsAsync();

                if (assetCount == 0)
                {
                    var batch = new List<MongoAssetEntity>();

                    async Task ExecuteBatchAsync(MongoAssetEntity? entity)
                    {
                        if (entity != null)
                        {
                            batch.Add(entity);
                        }

                        if ((entity == null || batch.Count >= 1000) && batch.Count > 0)
                        {
                            await collection.InsertManyAsync(batch);

                            batch.Clear();
                        }
                    }

                    foreach (var appId in AppIds)
                    {
                        for (var i = 0; i < numValues; i++)
                        {
                            var fileName = i.ToString(CultureInfo.InvariantCulture);

                            for (var j = 0; j < numValues; j++)
                            {
                                var tag = j.ToString(CultureInfo.InvariantCulture);

                                var asset = new MongoAssetEntity
                                {
                                    DocumentId = DomainId.NewGuid(),
                                    Tags = new HashSet<string> { tag },
                                    Id = DomainId.NewGuid(),
                                    FileHash = fileName,
                                    FileName = fileName,
                                    FileSize = 1024,
                                    IndexedAppId = appId.Id,
                                    IsDeleted = false,
                                    IsProtected = false,
                                    Metadata = new AssetMetadata
                                    {
                                        ["value"] = JsonValue.Create(tag)
                                    },
                                    Slug = fileName
                                };

                                await ExecuteBatchAsync(asset);
                            }
                        }
                    }

                    await ExecuteBatchAsync(null);
                }

                await mongoDatabase.RunCommandAsync<BsonDocument>("{ profile : 2 }");
            }).Wait();

            AssetRepository = assetRepository;
        }

        private static void SetupJson()
        {
            var jsonSerializer = JsonSerializer.Create(TestUtils.DefaultSerializerSettings);

            BsonJsonConvention.Register(jsonSerializer);
        }

        public DomainId RandomAppId()
        {
            return AppIds[random.Next(0, AppIds.Length)].Id;
        }

        public string RandomValue()
        {
            return random.Next(0, numValues).ToString(CultureInfo.InvariantCulture);
        }
    }
}
