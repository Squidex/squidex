// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    public sealed class ContentsQueryFixture
    {
        private readonly Random random = new Random();
        private readonly int numValues = 10000;
        private readonly IMongoClient mongoClient = new MongoClient("mongodb://localhost");
        private readonly IMongoDatabase mongoDatabase;

        public IContentRepository ContentRepository { get; }

        public NamedId<Guid>[] AppIds { get; } = new[]
        {
            NamedId.Of(Guid.Parse("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
            NamedId.Of(Guid.Parse("4b3672c1-97c6-4e0b-a067-71e9e9a29db9"), "my-app1")
        };

        public NamedId<Guid>[] SchemaIds { get; } = new[]
        {
            NamedId.Of(Guid.Parse("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-schema1"),
            NamedId.Of(Guid.Parse("4b3672c1-97c6-4e0b-a067-71e9e9a29db9"), "my-schema2"),
            NamedId.Of(Guid.Parse("76357c9b-0514-4377-9fcc-a632e7ef960d"), "my-schema3"),
            NamedId.Of(Guid.Parse("164c451e-e5a8-41f8-8aaf-e4b56603d7e7"), "my-schema4"),
            NamedId.Of(Guid.Parse("741e902c-fdfa-41ad-8e5a-b7cb9d6e3d94"), "my-schema5")
        };

        public ContentsQueryFixture()
        {
            mongoDatabase = mongoClient.GetDatabase("QueryTests");

            SetupJson();

            var contentRepository =
                new MongoContentRepository(
                    mongoDatabase,
                    CreateAppProvider(),
                    CreateTextIndexer(),
                    JsonHelper.DefaultSerializer);

            Task.Run(async () =>
            {
                await contentRepository.InitializeAsync();

                await mongoDatabase.RunCommandAsync<BsonDocument>("{ profile : 0 }");
                await mongoDatabase.DropCollectionAsync("system.profile");

                var collections = contentRepository.GetInternalCollections();

                foreach (var collection in collections)
                {
                    var contentCount = await collection.Find(new BsonDocument()).CountDocumentsAsync();

                    if (contentCount == 0)
                    {
                        var batch = new List<MongoContentEntity>();

                        async Task ExecuteBatchAsync(MongoContentEntity? entity)
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
                            foreach (var schemaId in SchemaIds)
                            {
                                for (var i = 0; i < numValues; i++)
                                {
                                    var value = i.ToString();

                                    var data =
                                        new IdContentData()
                                            .AddField(1,
                                                new ContentFieldData()
                                                    .AddJsonValue(JsonValue.Create(value)));

                                    var content = new MongoContentEntity
                                    {
                                        Id = Guid.NewGuid(),
                                        AppId = appId,
                                        DataByIds = data,
                                        IndexedAppId = appId.Id,
                                        IndexedSchemaId = schemaId.Id,
                                        IsDeleted = false,
                                        SchemaId = schemaId,
                                        Status = Status.Published
                                    };

                                    await ExecuteBatchAsync(content);
                                }
                            }
                        }

                        await ExecuteBatchAsync(null);
                    }
                }

                await mongoDatabase.RunCommandAsync<BsonDocument>("{ profile : 2 }");
            }).Wait();

            ContentRepository = contentRepository;
        }

        private static IAppProvider CreateAppProvider()
        {
            var appProvider = A.Fake<IAppProvider>();

            A.CallTo(() => appProvider.GetSchemaAsync(A<Guid>.Ignored, A<Guid>.Ignored, false))
                .ReturnsLazily(x => Task.FromResult<ISchemaEntity?>(CreateSchema(x.GetArgument<Guid>(0)!, x.GetArgument<Guid>(1)!)));

            return appProvider;
        }

        private static ITextIndexer CreateTextIndexer()
        {
            var textIndexer = A.Fake<ITextIndexer>();

            A.CallTo(() => textIndexer.SearchAsync(A<string>.Ignored, A<IAppEntity>.Ignored, A<Guid>.Ignored, A<SearchScope>.Ignored))
                .Returns(new List<Guid> { Guid.NewGuid() });

            return textIndexer;
        }

        private static void SetupJson()
        {
            var jsonSerializer = JsonSerializer.Create(JsonHelper.DefaultSettings());

            BsonJsonConvention.Register(jsonSerializer);
        }

        public Guid RandomAppId()
        {
            return AppIds[random.Next(0, AppIds.Length)].Id;
        }

        public IAppEntity RandomApp()
        {
            return CreateApp(RandomAppId());
        }

        public Guid RandomSchemaId()
        {
            return SchemaIds[random.Next(0, SchemaIds.Length)].Id;
        }

        public ISchemaEntity RandomSchema()
        {
            return CreateSchema(RandomAppId(), RandomSchemaId());
        }

        public string RandomValue()
        {
            return random.Next(0, numValues).ToString();
        }

        private static IAppEntity CreateApp(Guid appId)
        {
            return Mocks.App(NamedId.Of(appId, "my-app"));
        }

        private static ISchemaEntity CreateSchema(Guid appId, Guid schemaId)
        {
            var schemaDef =
                new Schema("my-schema")
                    .AddField(Fields.String(1, "value", Partitioning.Invariant));

            var schema =
                Mocks.Schema(
                    NamedId.Of(appId, "my-app"),
                    NamedId.Of(schemaId, "my-schema"),
                    schemaDef);

            return schema;
        }
    }
}
