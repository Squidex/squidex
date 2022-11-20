// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Assets;

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
    public async Task Should_return_empty_actuals_if_user_has_no_permission()
    {
        var ctx = ContextWithPermission();

        var actual = await sut.SearchAsync("logo", ctx, default);

        Assert.Empty(actual);

        A.CallTo(() => assetQuery.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_assets_actuals_if_found()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppAssetsRead, appId.Name);

        var ctx = ContextWithPermission(permission.Id);

        var asset1 = CreateAsset("logo1.png");
        var asset2 = CreateAsset("logo2.png");

        A.CallTo(() => urlGenerator.AssetsUI(appId, asset1.Id.ToString()))
            .Returns("assets-url1");

        A.CallTo(() => urlGenerator.AssetsUI(appId, asset2.Id.ToString()))
            .Returns("assets-url2");

        A.CallTo(() => assetQuery.QueryAsync(ctx, null, A<Q>.That.HasQuery("Filter: contains(fileName, 'logo'); Take: 5"), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(2, asset1, asset2));

        var actual = await sut.SearchAsync("logo", ctx, default);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("logo1.png", SearchResultType.Asset, "assets-url1")
                .Add("logo2.png", SearchResultType.Asset, "assets-url2"));
    }

    private static IEnrichedAssetEntity CreateAsset(string fileName)
    {
        return new AssetEntity { FileName = fileName, Id = DomainId.NewGuid() };
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
