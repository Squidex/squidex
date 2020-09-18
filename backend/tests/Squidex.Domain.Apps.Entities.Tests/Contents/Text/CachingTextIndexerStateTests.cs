// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class CachingTextIndexerStateTests
    {
        private readonly ITextIndexerState inner = A.Fake<ITextIndexerState>();
        private readonly CachingTextIndexerState sut;

        public CachingTextIndexerStateTests()
        {
            sut = new CachingTextIndexerState(inner);
        }

        [Fact]
        public async Task Should_retrieve_from_inner_when_not_cached()
        {
            var contentId = Guid.NewGuid();
            var contentIds = HashSet.Of(contentId);

            var state = new TextContentState { ContentId = contentId };

            var states = new Dictionary<Guid, TextContentState>
            {
                [contentId] = state
            };

            A.CallTo(() => inner.GetAsync(A<HashSet<Guid>>.That.Is(contentIds)))
                .Returns(states);

            var found1 = await sut.GetAsync(HashSet.Of(contentId));
            var found2 = await sut.GetAsync(HashSet.Of(contentId));

            Assert.Same(state, found1[contentId]);
            Assert.Same(state, found2[contentId]);

            A.CallTo(() => inner.GetAsync(A<HashSet<Guid>>.That.Is(contentIds)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_retrieve_from_inner_when_not_cached_and_not_found()
        {
            var contentId = Guid.NewGuid();
            var contentIds = HashSet.Of(contentId);

            var state = new TextContentState { ContentId = contentId };

            A.CallTo(() => inner.GetAsync(A<HashSet<Guid>>.That.Is(contentIds)))
                .Returns(new Dictionary<Guid, TextContentState>());

            var found1 = await sut.GetAsync(HashSet.Of(contentId));
            var found2 = await sut.GetAsync(HashSet.Of(contentId));

            Assert.Empty(found1);
            Assert.Empty(found2);

            A.CallTo(() => inner.GetAsync(A<HashSet<Guid>>.That.Is(contentIds)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_when_cached()
        {
            var contentId = Guid.NewGuid();
            var contentIds = HashSet.Of(contentId);

            var state = new TextContentState { ContentId = contentId };

            await sut.SetAsync(new List<TextContentState>
            {
                state
            });

            var found1 = await sut.GetAsync(contentIds);
            var found2 = await sut.GetAsync(contentIds);

            Assert.Same(state, found1[contentId]);
            Assert.Same(state, found2[contentId]);

            A.CallTo(() => inner.SetAsync(A<List<TextContentState>>.That.IsSameSequenceAs(state)))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(A<HashSet<Guid>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_when_removed()
        {
            var contentId = Guid.NewGuid();
            var contentIds = HashSet.Of(contentId);

            var state = new TextContentState { ContentId = contentId };

            await sut.SetAsync(new List<TextContentState>
            {
                state
            });

            await sut.SetAsync(new List<TextContentState>
            {
                new TextContentState { ContentId = contentId, IsDeleted = true }
            });

            var found1 = await sut.GetAsync(contentIds);
            var found2 = await sut.GetAsync(contentIds);

            Assert.Empty(found1);
            Assert.Empty(found2);

            A.CallTo(() => inner.SetAsync(A<List<TextContentState>>.That.Matches(x => x.Count == 1 && x[0].IsDeleted)))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(A<HashSet<Guid>>._))
                .MustNotHaveHappened();
        }
    }
}
