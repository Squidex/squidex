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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
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
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
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
                        ResolveFirst = true,
                        MinItems = 2,
                        MaxItems = 3
                    })
                    .AddAssets(2, "asset2", Partitioning.Language, new AssetsFieldProperties
                    {
                        ResolveFirst = true,
                        MinItems = 1,
                        MaxItems = 1
                    })
                    .SetFieldsInLists("asset1", "asset2");

            A.CallTo(() => urlGenerator.AssetContent(appId, A<string>._))
                .ReturnsLazily(ctx => $"url/to/{ctx.GetArgument<string>(1)}");

            schemaProvider = x =>
            {
                if (x == schemaId.Id)
                {
                    return Task.FromResult(Mocks.Schema(appId, schemaId, schemaDef));
                }
                else
                {
                    throw new DomainObjectNotFoundException(x.ToString());
                }
            };

            sut = new ResolveAssets(urlGenerator, assetQuery, requestCache);
        }

        [Fact]
        public async Task Should_add_assets_id_and_versions_as_dependency()
        {
            var document1 = CreateAsset(DomainId.NewGuid(), 3, AssetType.Unknown, "Document1.docx");
            var document2 = CreateAsset(DomainId.NewGuid(), 4, AssetType.Unknown, "Document2.docx");

            var contents = new[]
            {
                CreateContent(
                    new[] { document1.Id },
                    new[] { document1.Id }),
                CreateContent(
                    new[] { document2.Id },
                    new[] { document2.Id })
            };

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.That.Matches(x => !x.ShouldEnrichAsset()), null, A<Q>.That.Matches(x => x.Ids.Count == 2)))
                .Returns(ResultList.CreateFrom(4, document1, document2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            A.CallTo(() => requestCache.AddDependency(document1.UniqueId, document1.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(document2.UniqueId, document2.Version))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_with_asset_urls()
        {
            var image1 = CreateAsset(DomainId.NewGuid(), 1, AssetType.Image, "Image1.png");
            var image2 = CreateAsset(DomainId.NewGuid(), 2, AssetType.Image, "Image2.png");

            var document1 = CreateAsset(DomainId.NewGuid(), 3, AssetType.Unknown, "Document1.png");
            var document2 = CreateAsset(DomainId.NewGuid(), 4, AssetType.Unknown, "Document2.png");

            var contents = new[]
            {
                CreateContent(
                    new[] { image1.Id },
                    new[] { image2.Id, image1.Id }),
                CreateContent(
                    new[] { document1.Id },
                    new[] { document2.Id, document1.Id })
            };

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.That.Matches(x => !x.ShouldEnrichAsset()), null, A<Q>.That.Matches(x => x.Ids.Count == 4)))
                .Returns(ResultList.CreateFrom(4, image1, image2, document1, document2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            Assert.Equal(
                new NamedContentData()
                    .AddField("asset1",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array($"url/to/{image1.Id}", image1.FileName)))
                    .AddField("asset2",
                        new ContentFieldData()
                            .AddValue("en", JsonValue.Array($"url/to/{image2.Id}", image2.FileName))),
                contents[0].ReferenceData);

            Assert.Equal(
                new NamedContentData()
                    .AddField("asset1",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(document1.FileName)))
                    .AddField("asset2",
                        new ContentFieldData()
                            .AddValue("en", JsonValue.Array(document2.FileName))),
                contents[1].ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_references_if_not_api_user()
        {
            var contents = new[]
            {
                CreateContent(new[] { DomainId.NewGuid() }, Array.Empty<DomainId>())
            };

            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            await sut.EnrichAsync(ctx, contents, schemaProvider);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_references_if_disabled()
        {
            var contents = new[]
            {
                CreateContent(new[] { DomainId.NewGuid() }, Array.Empty<DomainId>())
            };

            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId)).WithoutContentEnrichment(true);

            await sut.EnrichAsync(ctx, contents, schemaProvider);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_no_assets_found()
        {
            var contents = new[]
            {
                CreateContent(Array.Empty<DomainId>(), Array.Empty<DomainId>())
            };

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            Assert.NotNull(contents[0].ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_only_query_first_assets()
        {
            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            var contents = new[]
            {
                CreateContent(new[] { id1, id2 }, Array.Empty<DomainId>())
            };

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            Assert.NotNull(contents[0].ReferenceData);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>.That.Matches(x => !x.ShouldEnrichAsset()), null, A<Q>.That.Matches(x => x.Ids.Count == 1)))
                .MustHaveHappened();
        }

        private ContentEntity CreateContent(DomainId[] assets1, DomainId[] assets2)
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

        private IEnrichedAssetEntity CreateAsset(DomainId id, int version, AssetType type, string fileName)
        {
            return new AssetEntity { AppId = appId, Id = id, Type = type, Version = version, FileName = fileName };
        }
    }
}
