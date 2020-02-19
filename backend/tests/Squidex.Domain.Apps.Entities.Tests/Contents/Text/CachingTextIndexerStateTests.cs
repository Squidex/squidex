// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
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

            var state = new TextContentState { ContentId = contentId };

            A.CallTo(() => inner.GetAsync(contentId))
                .Returns(state);

            var found1 = await sut.GetAsync(contentId);
            var found2 = await sut.GetAsync(contentId);

            Assert.Same(state, found1);
            Assert.Same(state, found2);

            A.CallTo(() => inner.GetAsync(contentId))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_when_cached()
        {
            var contentId = Guid.NewGuid();

            var state = new TextContentState { ContentId = contentId };

            await sut.SetAsync(state);

            var found1 = await sut.GetAsync(contentId);
            var found2 = await sut.GetAsync(contentId);

            Assert.Same(state, found1);
            Assert.Same(state, found2);

            A.CallTo(() => inner.SetAsync(state))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(contentId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_when_removed()
        {
            var contentId = Guid.NewGuid();

            var state = new TextContentState { ContentId = contentId };

            await sut.SetAsync(state);
            await sut.RemoveAsync(contentId);

            var found1 = await sut.GetAsync(contentId);
            var found2 = await sut.GetAsync(contentId);

            Assert.Null(found1);
            Assert.Null(found2);

            A.CallTo(() => inner.RemoveAsync(contentId))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(contentId))
                .MustNotHaveHappened();
        }
    }
}
