// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class CachingTextIndexerStateTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly ITextIndexerState inner = A.Fake<ITextIndexerState>();
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly CachingTextIndexerState sut;

        public CachingTextIndexerStateTests()
        {
            ct = cts.Token;

            sut = new CachingTextIndexerState(inner);
        }

        [Fact]
        public async Task Should_retrieve_from_inner_if_not_cached()
        {
            var contentIds = HashSet.Of(contentId);

            var state = new TextContentState { UniqueContentId = contentId };

            var states = new Dictionary<DomainId, TextContentState>
            {
                [contentId] = state
            };

            A.CallTo(() => inner.GetAsync(A<HashSet<DomainId>>.That.Is(contentIds), ct))
                .Returns(states);

            var found1 = await sut.GetAsync(HashSet.Of(contentId), ct);
            var found2 = await sut.GetAsync(HashSet.Of(contentId), ct);

            Assert.Same(state, found1[contentId]);
            Assert.Same(state, found2[contentId]);

            A.CallTo(() => inner.GetAsync(A<HashSet<DomainId>>.That.Is(contentIds), ct))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_retrieve_from_inner_if_not_cached_and_not_found()
        {
            var contentIds = HashSet.Of(contentId);

            A.CallTo(() => inner.GetAsync(A<HashSet<DomainId>>.That.Is(contentIds), ct))
                .Returns(new Dictionary<DomainId, TextContentState>());

            var found1 = await sut.GetAsync(HashSet.Of(contentId), ct);
            var found2 = await sut.GetAsync(HashSet.Of(contentId), ct);

            Assert.Empty(found1);
            Assert.Empty(found2);

            A.CallTo(() => inner.GetAsync(A<HashSet<DomainId>>.That.Is(contentIds), ct))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_if_cached()
        {
            var contentIds = HashSet.Of(contentId);

            var state = new TextContentState { UniqueContentId = contentId };

            await sut.SetAsync(new List<TextContentState> { state }, ct);

            var found1 = await sut.GetAsync(contentIds, ct);
            var found2 = await sut.GetAsync(contentIds, ct);

            Assert.Same(state, found1[contentId]);
            Assert.Same(state, found2[contentId]);

            A.CallTo(() => inner.SetAsync(A<List<TextContentState>>.That.IsSameSequenceAs(state), ct))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(A<HashSet<DomainId>>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_if_removed()
        {
            var contentIds = HashSet.Of(contentId);

            var state = new TextContentState { UniqueContentId = contentId };

            await sut.SetAsync(new List<TextContentState>
            {
                state
            }, ct);

            await sut.SetAsync(new List<TextContentState>
            {
                new TextContentState { UniqueContentId = contentId, IsDeleted = true }
            }, ct);

            var found1 = await sut.GetAsync(contentIds, ct);
            var found2 = await sut.GetAsync(contentIds, ct);

            Assert.Empty(found1);
            Assert.Empty(found2);

            A.CallTo(() => inner.SetAsync(A<List<TextContentState>>.That.Matches(x => x.Count == 1 && x[0].IsDeleted), ct))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(A<HashSet<DomainId>>._, ct))
                .MustNotHaveHappened();
        }
    }
}
