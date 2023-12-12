// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetsSearchSourceTests : GivenContext
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly AssetsSearchSource sut;

    public AssetsSearchSourceTests()
    {
        sut = new AssetsSearchSource(assetQuery, urlGenerator);
    }

    [Fact]
    public async Task Should_return_empty_results_if_user_has_no_permission()
    {
        var actual = await sut.SearchAsync("logo", ApiContext, default);

        Assert.Empty(actual);

        A.CallTo(() => assetQuery.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_assets_results_if_found()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppAssetsRead, AppId.Name);

        var requestContext = CreateContext(false, permission.Id);

        var asset1 = CreateAsset() with { FileName = "logo1.png" };
        var asset2 = CreateAsset() with { FileName = "logo2.png" };

        A.CallTo(() => urlGenerator.AssetsUI(AppId, asset1.Id.ToString()))
            .Returns("assets-url1");

        A.CallTo(() => urlGenerator.AssetsUI(AppId, asset2.Id.ToString()))
            .Returns("assets-url2");

        A.CallTo(() => assetQuery.QueryAsync(requestContext, null, A<Q>.That.HasQuery("Filter: contains(fileName, 'logo'); Take: 5"), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(2, asset1, asset2));

        var actual = await sut.SearchAsync("logo", requestContext, default);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("logo1.png", SearchResultType.Asset, "assets-url1")
                .Add("logo2.png", SearchResultType.Asset, "assets-url2"));
    }
}
