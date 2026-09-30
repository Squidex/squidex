// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class AssetQueryCacheInvalidatorTests : GivenContext
{
    private readonly ICacheGenerations generations = A.Fake<ICacheGenerations>();
    private readonly AssetQueryCacheInvalidator sut;

    public AssetQueryCacheInvalidatorTests()
    {
        sut = new AssetQueryCacheInvalidator(generations);
    }

    [Fact]
    public async Task Should_reset_generation_if_asset_changed()
    {
        await HandleAsync(new AnnotateAsset { AppId = AppId });

        A.CallTo(() => generations.ResetAsync(CachingAssetQueryService.GenerationKey(AppId.Id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_folder_changed()
    {
        await HandleAsync(new RenameAssetFolder { AppId = AppId });

        A.CallTo(() => generations.ResetAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_command_failed()
    {
        var commandContext = new CommandContext(new AnnotateAsset { AppId = AppId }, A.Fake<ICommandBus>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.HandleAsync(commandContext, (_, _) => throw new InvalidOperationException(), CancellationToken));

        A.CallTo(() => generations.ResetAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private Task HandleAsync(ICommand command)
    {
        return sut.HandleAsync(new CommandContext(command, A.Fake<ICommandBus>()), (_, _) => Task.CompletedTask, CancellationToken);
    }
}
