// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ContentQueryCacheInvalidatorTests : GivenContext
{
    private readonly ICacheGenerations generations = A.Fake<ICacheGenerations>();
    private readonly ContentQueryCacheInvalidator sut;

    public ContentQueryCacheInvalidatorTests()
    {
        sut = new ContentQueryCacheInvalidator(generations);
    }

    [Fact]
    public async Task Should_reset_generation_if_content_changed()
    {
        await HandleAsync(new UpdateContent { AppId = AppId });

        A.CallTo(() => generations.Reset(CachingContentQueryService.GenerationKey(AppId.Id)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_reset_generation_if_asset_deleted()
    {
        await HandleAsync(new DeleteAsset { AppId = AppId });

        A.CallTo(() => generations.Reset(CachingContentQueryService.GenerationKey(AppId.Id)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_content_is_only_validated()
    {
        await HandleAsync(new ValidateContent { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_content_defaults_are_only_enriched()
    {
        await HandleAsync(new EnrichContentDefaults { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_asset_changed()
    {
        await HandleAsync(new AnnotateAsset { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_command_failed()
    {
        var commandContext = new CommandContext(new UpdateContent { AppId = AppId }, A.Fake<ICommandBus>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.HandleAsync(commandContext, (_, _) => throw new InvalidOperationException(), CancellationToken));

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    private Task HandleAsync(ICommand command)
    {
        return sut.HandleAsync(new CommandContext(command, A.Fake<ICommandBus>()), (_, _) => Task.CompletedTask, CancellationToken);
    }
}
