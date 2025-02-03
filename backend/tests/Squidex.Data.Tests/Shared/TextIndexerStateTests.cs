// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one

namespace Squidex.Shared;

public abstract class TextIndexerStateTests : GivenContext
{
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();

    protected abstract Task<ITextIndexerState> CreateSutAsync(IContentRepository contentRepository);

    [Fact]
    public async Task Should_add_state()
    {
        var sut = await CreateSutAsync(contentRepository);

        var id1 = new UniqueContentId(AppId.Id, DomainId.NewGuid());
        var id2 = new UniqueContentId(AppId.Id, DomainId.NewGuid());
        var id3 = new UniqueContentId(AppId.Id, DomainId.NewGuid());

        await sut.SetAsync(
        [
            new TextContentState { UniqueContentId = id1, State = TextState.Stage0_Draft__Stage1_None },
            new TextContentState { UniqueContentId = id2, State = TextState.Stage0_Published__Stage1_Draft },
            new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None },
        ]);

        var actual = await sut.GetAsync(HashSet.Of(id1, id2));

        actual.Should().BeEquivalentTo(new Dictionary<UniqueContentId, TextContentState>
        {
            [id1] = new TextContentState { UniqueContentId = id1, State = TextState.Stage0_Draft__Stage1_None },
            [id2] = new TextContentState { UniqueContentId = id2, State = TextState.Stage0_Published__Stage1_Draft },
        });
    }

    [Fact]
    public async Task Should_remove_state()
    {
        var sut = await CreateSutAsync(contentRepository);

        var id = new UniqueContentId(DomainId.NewGuid(), DomainId.NewGuid());

        await sut.SetAsync(
        [
            new TextContentState { UniqueContentId = id, State = TextState.Stage0_Draft__Stage1_None },
        ]);

        await sut.SetAsync(
        [
            new TextContentState { UniqueContentId = id, State = TextState.Deleted },
        ]);

        var actual = await sut.GetAsync(HashSet.Of(id));

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_remove_by_app()
    {
        var sut = await CreateSutAsync(contentRepository);
        if (sut is not IDeleter deleter)
        {
            return;
        }

        var app1 = new App { Id = DomainId.NewGuid() };
        var app2 = new App { Id = DomainId.NewGuid() };

        var id1 = new UniqueContentId(app1.Id, DomainId.NewGuid());
        var id2 = new UniqueContentId(app1.Id, DomainId.NewGuid());
        var id3 = new UniqueContentId(app2.Id, DomainId.NewGuid());

        await sut.SetAsync(
        [
            new TextContentState { UniqueContentId = id1, State = TextState.Stage0_Draft__Stage1_None },
            new TextContentState { UniqueContentId = id2, State = TextState.Stage0_Published__Stage1_Draft },
            new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None },
        ]);

        A.CallTo(() => contentRepository.StreamIds(app1.Id, null, SearchScope.All, default))
            .Returns(new[] { id1.ContentId, id2.ContentId }.ToAsyncEnumerable());

        await deleter.DeleteAppAsync(app1, default);

        var actual = await sut.GetAsync(HashSet.Of(id1, id2, id3));

        actual.Should().BeEquivalentTo(new Dictionary<UniqueContentId, TextContentState>
        {
            [id3] = new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None },
        });
    }

    [Fact]
    public async Task Should_remove_by_schema()
    {
        var sut = await CreateSutAsync(contentRepository);
        if (sut is not IDeleter deleter)
        {
            return;
        }

        var id1 = new UniqueContentId(AppId.Id, DomainId.NewGuid());
        var id2 = new UniqueContentId(AppId.Id, DomainId.NewGuid());
        var id3 = new UniqueContentId(AppId.Id, DomainId.NewGuid());

        await sut.SetAsync(
        [
            new TextContentState { UniqueContentId = id1, State = TextState.Stage0_Draft__Stage1_None },
            new TextContentState { UniqueContentId = id2, State = TextState.Stage0_Published__Stage1_Draft },
            new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None },
        ]);

        A.CallTo(() => contentRepository.StreamIds(AppId.Id, A<HashSet<DomainId>>.That.Is(Schema.Id), SearchScope.All, default))
            .Returns(new[] { id1.ContentId, id2.ContentId }.ToAsyncEnumerable());

        await deleter.DeleteSchemaAsync(App, Schema, default);

        var actual = await sut.GetAsync(HashSet.Of(id1, id2, id3));

        actual.Should().BeEquivalentTo(new Dictionary<UniqueContentId, TextContentState>
        {
            [id3] = new TextContentState { UniqueContentId = id3, State = TextState.Stage0_Published__Stage1_None },
        });
    }
}
