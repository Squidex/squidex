// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class CachingTextIndexerStateTests : GivenContext
{
    private readonly ITextIndexerState inner = A.Fake<ITextIndexerState>();
    private readonly UniqueContentId contentId;
    private readonly CachingTextIndexerState sut;

    public CachingTextIndexerStateTests()
    {
        contentId = new UniqueContentId(AppId.Id, DomainId.NewGuid());

        sut = new CachingTextIndexerState(inner);
    }

    [Fact]
    public async Task Should_retrieve_from_inner_if_not_cached()
    {
        var contentIds = HashSet.Of(contentId);

        var state = new TextContentState { UniqueContentId = contentId };

        var states = new Dictionary<UniqueContentId, TextContentState>
        {
            [contentId] = state
        };

        A.CallTo(() => inner.GetAsync(A<HashSet<UniqueContentId>>.That.Is(contentIds), CancellationToken))
            .Returns(states);

        var found1 = await sut.GetAsync(HashSet.Of(contentId), CancellationToken);
        var found2 = await sut.GetAsync(HashSet.Of(contentId), CancellationToken);

        Assert.Same(state, found1[contentId]);
        Assert.Same(state, found2[contentId]);

        A.CallTo(() => inner.GetAsync(A<HashSet<UniqueContentId>>.That.Is(contentIds), CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_retrieve_from_inner_if_not_cached_and_not_found()
    {
        var contentIds = HashSet.Of(contentId);

        A.CallTo(() => inner.GetAsync(A<HashSet<UniqueContentId>>.That.Is(contentIds), CancellationToken))
            .Returns([]);

        var found1 = await sut.GetAsync(HashSet.Of(contentId), CancellationToken);
        var found2 = await sut.GetAsync(HashSet.Of(contentId), CancellationToken);

        Assert.Empty(found1);
        Assert.Empty(found2);

        A.CallTo(() => inner.GetAsync(A<HashSet<UniqueContentId>>.That.Is(contentIds), CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_retrieve_from_inner_if_cached()
    {
        var contentIds = HashSet.Of(contentId);

        var state = new TextContentState { UniqueContentId = contentId };

        await sut.SetAsync([state], CancellationToken);

        var found1 = await sut.GetAsync(contentIds, CancellationToken);
        var found2 = await sut.GetAsync(contentIds, CancellationToken);

        Assert.Same(state, found1[contentId]);
        Assert.Same(state, found2[contentId]);

        A.CallTo(() => inner.SetAsync(A<List<TextContentState>>.That.IsSameSequenceAs(state), CancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => inner.GetAsync(A<HashSet<UniqueContentId>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_retrieve_from_inner_if_removed()
    {
        var contentIds = HashSet.Of(contentId);

        var state = new TextContentState { UniqueContentId = contentId };

        await sut.SetAsync(
        [
            state
        ], CancellationToken);

        await sut.SetAsync(
        [
            new TextContentState { UniqueContentId = contentId, State = TextState.Deleted }
        ], CancellationToken);

        var found1 = await sut.GetAsync(contentIds, CancellationToken);
        var found2 = await sut.GetAsync(contentIds, CancellationToken);

        Assert.Empty(found1);
        Assert.Empty(found2);

        A.CallTo(() => inner.SetAsync(A<List<TextContentState>>.That.Matches(x => x.Count == 1 && x[0].State == TextState.Deleted), CancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => inner.GetAsync(A<HashSet<UniqueContentId>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
