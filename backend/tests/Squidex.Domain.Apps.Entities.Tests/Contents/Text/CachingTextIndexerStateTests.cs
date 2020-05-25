// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class CachingTextIndexerStateTests
    {
        private readonly ITextIndexerState inner = A.Fake<ITextIndexerState>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly CachingTextIndexerState sut;

        public CachingTextIndexerStateTests()
        {
            sut = new CachingTextIndexerState(inner);
        }

        [Fact]
        public async Task Should_retrieve_from_inner_when_not_cached()
        {
            var state = new TextContentState { ContentId = contentId };

            A.CallTo(() => inner.GetAsync(appId, contentId))
                .Returns(state);

            var found1 = await sut.GetAsync(appId, contentId);
            var found2 = await sut.GetAsync(appId, contentId);

            Assert.Same(state, found1);
            Assert.Same(state, found2);

            A.CallTo(() => inner.GetAsync(appId, contentId))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_when_cached()
        {
            var state = new TextContentState { ContentId = contentId };

            await sut.SetAsync(appId, state);

            var found1 = await sut.GetAsync(appId, contentId);
            var found2 = await sut.GetAsync(appId, contentId);

            Assert.Same(state, found1);
            Assert.Same(state, found2);

            A.CallTo(() => inner.SetAsync(appId, state))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(appId, contentId))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_retrieve_from_inner_when_removed()
        {
            var state = new TextContentState { ContentId = contentId };

            await sut.SetAsync(appId, state);

            await sut.RemoveAsync(appId, contentId);

            var found1 = await sut.GetAsync(appId, contentId);
            var found2 = await sut.GetAsync(appId, contentId);

            Assert.Null(found1);
            Assert.Null(found2);

            A.CallTo(() => inner.RemoveAsync(appId, contentId))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => inner.GetAsync(appId, contentId))
                .MustNotHaveHappened();
        }
    }
}
