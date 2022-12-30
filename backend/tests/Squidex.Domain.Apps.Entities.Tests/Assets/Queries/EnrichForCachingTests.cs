// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class EnrichForCachingTests : GivenContext
{
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly EnrichForCaching sut;

    public EnrichForCachingTests()
    {
        sut = new EnrichForCaching(requestCache);
    }

    [Fact]
    public async Task Should_add_cache_headers()
    {
        var headers = new List<string>();

        A.CallTo(() => requestCache.AddHeader(A<string>._))
            .Invokes(new Action<string>(header => headers.Add(header)));

        await sut.EnrichAsync(ApiContext, CancellationToken);

        Assert.Equal(new List<string>
        {
            "X-Flatten",
            "X-Languages",
            "X-NoCleanup",
            "X-NoEnrichment",
            "X-NoResolveLanguages",
            "X-ResolveFlow",
            "X-Resolve-Urls",
            "X-Unpublished"
        }, headers);
    }

    [Fact]
    public async Task Should_add_app_version_as_dependency()
    {
        var asset = CreateAsset();

        await sut.EnrichAsync(ApiContext, Enumerable.Repeat(asset, 1), CancellationToken);

        A.CallTo(() => requestCache.AddDependency(asset.UniqueId, asset.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(App.UniqueId, App.Version))
            .MustHaveHappened();
    }

    private AssetEntity CreateAsset()
    {
        return new AssetEntity { AppId = AppId, Id = DomainId.NewGuid(), Version = 13 };
    }
}
