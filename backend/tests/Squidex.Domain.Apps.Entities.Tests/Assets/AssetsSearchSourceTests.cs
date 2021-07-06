// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetsSearchSourceTests
    {
        private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly AssetsSearchSource sut;

        public AssetsSearchSourceTests()
        {
            sut = new AssetsSearchSource(assetQuery, urlGenerator);
        }

        [Fact]
        public async Task Should_return_empty_results_if_user_has_no_permission()
        {
            var ctx = ContextWithPermission();

            var result = await sut.SearchAsync("logo", ctx, default);

            Assert.Empty(result);

            A.CallTo(() => assetQuery.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_assets_results_if_found()
        {
            var permission = Permissions.ForApp(Permissions.AppAssetsRead, appId.Name);

            var ctx = ContextWithPermission(permission.Id);

            var asset1 = CreateAsset("logo-light.png");
            var asset2 = CreateAsset("logo-dark.png");

            A.CallTo(() => urlGenerator.AssetsUI(appId, "logo"))
                .Returns("assets-url");

            A.CallTo(() => assetQuery.QueryAsync(ctx, null, A<Q>.That.HasQuery("Filter: contains(fileName, 'logo'); Take: 5"), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(2, asset1, asset2));

            var result = await sut.SearchAsync("logo", ctx, default);

            result.Should().BeEquivalentTo(
                new SearchResults()
                    .Add("logo-light.png", SearchResultType.Asset, "assets-url")
                    .Add("logo-dark.png", SearchResultType.Asset, "assets-url"));
        }

        private static IEnrichedAssetEntity CreateAsset(string fileName)
        {
            return new AssetEntity { FileName = fileName };
        }

        private Context ContextWithPermission(string? permission = null)
        {
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            if (permission != null)
            {
                claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));
            }

            return new Context(claimsPrincipal, Mocks.App(appId));
        }
    }
}
