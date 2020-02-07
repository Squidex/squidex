// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ResolveAssetsTests
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetUrlGenerator assetUrlGenerator = A.Fake<IAssetUrlGenerator>();
        private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ProvideSchema schemaProvider;
        private readonly Context requestContext;
        private readonly ResolveAssets sut;

        public ResolveAssetsTests()
        {
            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId, Language.DE));

            var schemaDef =
                new Schema(schemaId.Name)
                    .AddAssets(1, "asset1", Partitioning.Invariant, new AssetsFieldProperties
                    {
                        ResolveImage = true,
                        MinItems = 2,
                        MaxItems = 3
                    })
                    .AddAssets(2, "asset2", Partitioning.Language, new AssetsFieldProperties
                    {
                        ResolveImage = true,
                        MinItems = 1,
                        MaxItems = 1
                    })
                    .SetFieldsInLists("asset1", "asset2");

            A.CallTo(() => assetUrlGenerator.GenerateUrl(A<string>.Ignored))
                .ReturnsLazily(new Func<string, string>(id => $"url/to/{id}"));

            schemaProvider = x =>
            {
                if (x == schemaId.Id)
                {
                    return Task.FromResult(Mocks.Schema(appId, schemaId, schemaDef));
                }
                else
                {
                    throw new DomainObjectNotFoundException(x.ToString(), typeof(ISchemaEntity));
                }
            };

            sut = new ResolveAssets(assetUrlGenerator, assetQuery, requestCache);
        }

        [Fact]
        public async Task Should_add_assets_id_and_versions_as_dependency()
        {
            var image1 = CreateAsset(Guid.NewGuid(), 1, AssetType.Image);
            var image2 = CreateAsset(Guid.NewGuid(), 2, AssetType.Image);

            var document1 = CreateAsset(Guid.NewGuid(), 3, AssetType.Unknown);
            var document2 = CreateAsset(Guid.NewGuid(), 4, AssetType.Unknown);

            var contents = new[]
            {
                CreateContent(
                    new[] { document1.Id, image1.Id },
                    new[] { document1.Id }),
                CreateContent(
                    new[] { document1.Id },
                    new[] { document2.Id, image2.Id })
            };

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.That.Matches(x => !x.ShouldEnrichAsset()), null, A<Q>.That.Matches(x => x.Ids.Count == 4)))
                .Returns(ResultList.CreateFrom(4, image1, image2, document1, document2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            A.CallTo(() => requestCache.AddDependency(image1.Id, image1.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(image2.Id, image2.Version))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_with_asset_urls()
        {
            var image1 = CreateAsset(Guid.NewGuid(), 1, AssetType.Image);
            var image2 = CreateAsset(Guid.NewGuid(), 2, AssetType.Image);

            var document1 = CreateAsset(Guid.NewGuid(), 3, AssetType.Unknown);
            var document2 = CreateAsset(Guid.NewGuid(), 4, AssetType.Unknown);

            var contents = new[]
            {
                CreateContent(
                    new[] { document1.Id, image1.Id },
                    new[] { document1.Id }),
                CreateContent(
                    new[] { document1.Id },
                    new[] { document2.Id, image2.Id })
            };

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.That.Matches(x => !x.ShouldEnrichAsset()), null, A<Q>.That.Matches(x => x.Ids.Count == 4)))
                .Returns(ResultList.CreateFrom(4, image1, image2, document1, document2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            Assert.Equal(
                new NamedContentData()
                    .AddField("asset1",
                        new ContentFieldData()
                            .AddValue("iv", $"url/to/{image1.Id}"))
                    .AddField("asset2",
                        new ContentFieldData()),
                contents[0].ReferenceData);

            Assert.Equal(
                new NamedContentData()
                    .AddField("asset1",
                        new ContentFieldData())
                    .AddField("asset2",
                        new ContentFieldData()
                            .AddValue("en", $"url/to/{image2.Id}")),
                contents[1].ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_references_if_not_api_user()
        {
            var contents = new[]
            {
                CreateContent(new[] { Guid.NewGuid() }, new Guid[0])
            };

            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            await sut.EnrichAsync(ctx, contents, schemaProvider);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.Ignored, null, A<Q>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_references_if_disabled()
        {
            var contents = new[]
            {
                CreateContent(new[] { Guid.NewGuid() }, new Guid[0])
            };

            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId)).WithoutContentEnrichment(true);

            await sut.EnrichAsync(ctx, contents, schemaProvider);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.Ignored, null, A<Q>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_no_assets_found()
        {
            var contents = new[]
            {
                CreateContent(new Guid[0], new Guid[0])
            };

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            Assert.NotNull(contents[0].ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.Ignored, null, A<Q>.Ignored))
                .MustNotHaveHappened();
        }

        private ContentEntity CreateContent(Guid[] assets1, Guid[] assets2)
        {
            return new ContentEntity
            {
                Data =
                    new NamedContentData()
                        .AddField("asset1",
                            new ContentFieldData()
                                .AddJsonValue("iv", JsonValue.Array(assets1.Select(x => x.ToString()).ToArray())))
                        .AddField("asset2",
                            new ContentFieldData()
                                .AddJsonValue("en", JsonValue.Array(assets2.Select(x => x.ToString()).ToArray()))),
                SchemaId = schemaId
            };
        }

        private static IEnrichedAssetEntity CreateAsset(Guid id, int version, AssetType type)
        {
            return new AssetEntity { Id = id, Type = type, Version = version };
        }
    }
}
