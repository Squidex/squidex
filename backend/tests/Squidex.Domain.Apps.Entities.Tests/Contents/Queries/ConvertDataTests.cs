// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ConvertDataTests
    {
        private readonly ISchemaEntity schema;
        private readonly IAssetUrlGenerator assetUrlGenerator = A.Fake<IAssetUrlGenerator>();
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ProvideSchema schemaProvider;
        private readonly ConvertData sut;

        public ConvertDataTests()
        {
            var schemaDef =
                new Schema("my-schema")
                    .AddReferences(1, "references", Partitioning.Invariant)
                    .AddAssets(2, "assets", Partitioning.Invariant)
                    .AddArray(3, "array", Partitioning.Invariant, a => a
                        .AddAssets(31, "nested"));

            schema = Mocks.Schema(appId, schemaId, schemaDef);
            schemaProvider = x => Task.FromResult(schema);

            sut = new ConvertData(assetUrlGenerator, assetRepository, contentRepository);
        }

        [Fact]
        public async Task Should_convert_data_and_data_draft_when_frontend_user()
        {
            var content = CreateContent(new NamedContentData());

            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            await sut.EnrichAsync(ctx, Enumerable.Repeat(content, 1), schemaProvider);

            Assert.NotNull(content.Data);
        }

        [Fact]
        public async Task Should_cleanup_references()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var source = BuildTestData(id1, id2);

            var content = CreateContent(source);

            var expected =
                new NamedContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddJsonValue(JsonValue.Array(id2)))
                    .AddField("assets",
                        new ContentFieldData()
                            .AddJsonValue(JsonValue.Array()))
                    .AddField("array",
                        new ContentFieldData()
                            .AddJsonValue(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add("nested", JsonValue.Array(id2)))));

            A.CallTo(() => assetRepository.QueryIdsAsync(appId.Id, A<HashSet<Guid>>.That.Is(id1, id2)))
                .Returns(new List<Guid> { id2 });

            A.CallTo(() => contentRepository.QueryIdsAsync(appId.Id, A<HashSet<Guid>>.That.Is(id1, id2), SearchScope.All))
                .Returns(new List<(Guid, Guid)> { (id2, id2) });

            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            await sut.EnrichAsync(ctx, Enumerable.Repeat(content, 1), schemaProvider);

            Assert.Equal(expected, content.Data);
        }

        [Fact]
        public async Task Should_cleanup_references_when_everything_deleted()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var source = BuildTestData(id1, id2);

            var content = CreateContent(source);

            var expected =
                new NamedContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddJsonValue(JsonValue.Array()))
                    .AddField("assets",
                        new ContentFieldData()
                            .AddJsonValue(JsonValue.Array()))
                    .AddField("array",
                        new ContentFieldData()
                            .AddJsonValue(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add("nested", JsonValue.Array()))));

            A.CallTo(() => assetRepository.QueryIdsAsync(appId.Id, A<HashSet<Guid>>.That.Is(id1, id2)))
                .Returns(new List<Guid>());

            A.CallTo(() => contentRepository.QueryIdsAsync(appId.Id, A<HashSet<Guid>>.That.Is(id1, id2), SearchScope.All))
                .Returns(new List<(Guid, Guid)>());

            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

            await sut.EnrichAsync(ctx, Enumerable.Repeat(content, 1), schemaProvider);

            Assert.Equal(expected, content.Data);
        }

        private static NamedContentData BuildTestData(Guid id1, Guid id2)
        {
            return new NamedContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddJsonValue(JsonValue.Array(id1, id2)))
                .AddField("assets",
                    new ContentFieldData()
                        .AddJsonValue(JsonValue.Array(id1)))
                .AddField("array",
                    new ContentFieldData()
                        .AddJsonValue(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested", JsonValue.Array(id1, id2)))));
        }

        private ContentEntity CreateContent(NamedContentData data)
        {
            return new ContentEntity
            {
                Data = data,
                SchemaId = schemaId,
                Status = Status.Published
            };
        }
    }
}
