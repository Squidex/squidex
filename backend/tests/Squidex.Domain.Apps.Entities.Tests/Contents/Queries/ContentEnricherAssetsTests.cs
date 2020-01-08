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
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentEnricherAssetsTests
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetUrlGenerator assetUrlGenerator = A.Fake<IAssetUrlGenerator>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly Context requestContext;
        private readonly ContentEnricher sut;

        public ContentEnricherAssetsTests()
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
                    .ConfigureFieldsInLists("asset1", "asset2");

            A.CallTo(() => assetUrlGenerator.GenerateUrl(A<string>.Ignored))
                .ReturnsLazily(new Func<string, string>(id => $"url/to/{id}"));

            void SetupSchema(NamedId<Guid> id, Schema def)
            {
                var schemaEntity = Mocks.Schema(appId, id, def);

                A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(requestContext, id.Id.ToString()))
                    .Returns(schemaEntity);
            }

            SetupSchema(schemaId, schemaDef);

            sut = new ContentEnricher(assetQuery, assetUrlGenerator, new Lazy<IContentQueryService>(() => contentQuery), contentWorkflow);
        }

        [Fact]
        public async Task Should_add_assets_id_and_versions_as_dependency()
        {
            var image1 = CreateAsset(Guid.NewGuid(), 1, AssetType.Image);
            var image2 = CreateAsset(Guid.NewGuid(), 2, AssetType.Image);

            var document1 = CreateAsset(Guid.NewGuid(), 3, AssetType.Unknown);
            var document2 = CreateAsset(Guid.NewGuid(), 4, AssetType.Unknown);

            var source = new IContentEntity[]
            {
                CreateContent(
                    new[] { document1.Id, image1.Id },
                    new[] { document1.Id }),
                CreateContent(
                    new[] { document1.Id },
                    new[] { document2.Id, image2.Id })
            };

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.That.Matches(x => x.IsNoAssetEnrichment()), null, A<Q>.That.Matches(x => x.Ids.Count == 4)))
                .Returns(ResultList.CreateFrom(4, image1, image2, document1, document2));

            var enriched = await sut.EnrichAsync(source, requestContext);

            var enriched1 = enriched.ElementAt(0);

            Assert.Contains(image1.Id, enriched1.CacheDependencies);
            Assert.Contains(image1.Version, enriched1.CacheDependencies);

            var enriched2 = enriched.ElementAt(1);

            Assert.Contains(image2.Id, enriched2.CacheDependencies);
            Assert.Contains(image2.Version, enriched2.CacheDependencies);
        }

        [Fact]
        public async Task Should_enrich_with_asset_urls()
        {
            var image1 = CreateAsset(Guid.NewGuid(), 1, AssetType.Image);
            var image2 = CreateAsset(Guid.NewGuid(), 2, AssetType.Image);

            var document1 = CreateAsset(Guid.NewGuid(), 3, AssetType.Unknown);
            var document2 = CreateAsset(Guid.NewGuid(), 4, AssetType.Unknown);

            var source = new IContentEntity[]
            {
                CreateContent(
                    new[] { document1.Id, image1.Id },
                    new[] { document1.Id }),
                CreateContent(
                    new[] { document1.Id },
                    new[] { document2.Id, image2.Id })
            };

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.That.Matches(x => x.IsNoAssetEnrichment()), null, A<Q>.That.Matches(x => x.Ids.Count == 4)))
                .Returns(ResultList.CreateFrom(4, image1, image2, document1, document2));

            var enriched = await sut.EnrichAsync(source, requestContext);

            Assert.Equal(
                new NamedContentData()
                    .AddField("asset1",
                        new ContentFieldData()
                            .AddValue("iv",
                                $"url/to/{image1.Id}"))
                    .AddField("asset2",
                        new ContentFieldData()),
                enriched.ElementAt(0).ReferenceData);

            Assert.Equal(
                new NamedContentData()
                    .AddField("asset1",
                        new ContentFieldData())
                    .AddField("asset2",
                        new ContentFieldData()
                            .AddValue("en",
                                $"url/to/{image2.Id}")),
                enriched.ElementAt(1).ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_references_if_not_api_user()
        {
            var source = new IContentEntity[]
            {
                CreateContent(new Guid[] { Guid.NewGuid() }, new Guid[0])
            };

            var enriched = await sut.EnrichAsync(source, new Context(Mocks.ApiUser(), Mocks.App(appId)));

            Assert.Null(enriched.ElementAt(0).ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.Ignored, null, A<Q>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_references_if_disabled()
        {
            var source = new IContentEntity[]
            {
                CreateContent(new Guid[] { Guid.NewGuid() }, new Guid[0])
            };

            var enriched = await sut.EnrichAsync(source, new Context(Mocks.ApiUser(), Mocks.App(appId)));

            Assert.Null(enriched.ElementAt(0).ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.Ignored, null, A<Q>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_no_assets_found()
        {
            var source = new IContentEntity[]
            {
                CreateContent(new Guid[0], new Guid[0])
            };

            var enriched = await sut.EnrichAsync(source, requestContext);

            Assert.NotNull(enriched.ElementAt(0).ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.Ignored, null, A<Q>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_nothing_to_enrich()
        {
            var source = new IContentEntity[0];

            await sut.EnrichAsync(source, requestContext);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.Ignored, null, A<Q>.Ignored))
                .MustNotHaveHappened();
        }

        private IEnrichedContentEntity CreateContent(Guid[] assets1, Guid[] assets2)
        {
            return new ContentEntity
            {
                DataDraft =
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
