// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetTagsDeleterTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly AssetTagsDeleter sut;

    public AssetTagsDeleterTests()
    {
        ct = cts.Token;

        sut = new AssetTagsDeleter(tagService);
    }

    [Fact]
    public void Should_run_with_default_order()
    {
        var order = ((IDeleter)sut).Order;

        Assert.Equal(0, order);
    }

    [Fact]
    public async Task Should_remove_events_from_streams()
    {
        var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

        await sut.DeleteAppAsync(app, ct);

        A.CallTo(() => tagService.ClearAsync(app.Id, TagGroups.Assets, ct))
            .MustHaveHappened();
    }
}
