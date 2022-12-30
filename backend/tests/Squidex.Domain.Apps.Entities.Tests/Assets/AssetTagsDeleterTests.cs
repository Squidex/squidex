// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetTagsDeleterTests : GivenContext
{
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly AssetTagsDeleter sut;

    public AssetTagsDeleterTests()
    {
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
        await sut.DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => tagService.ClearAsync(AppId.Id, TagGroups.Assets, CancellationToken))
            .MustHaveHappened();
    }
}
