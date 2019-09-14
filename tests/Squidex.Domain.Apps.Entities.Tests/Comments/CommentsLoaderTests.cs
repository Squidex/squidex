// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class CommentsLoaderTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly CommentsLoader sut;

        public CommentsLoaderTests()
        {
            sut = new CommentsLoader(grainFactory);
        }

        [Fact]
        public async Task Should_get_comments_from_grain()
        {
            var commentsId = Guid.NewGuid();
            var comments = new CommentsResult();

            var grain = A.Fake<ICommentsGrain>();

            A.CallTo(() => grain.GetCommentsAsync(11))
                .Returns(comments);

            A.CallTo(() => grainFactory.GetGrain<ICommentsGrain>(commentsId, null))
                .Returns(grain);

            var result = await sut.GetCommentsAsync(commentsId, 11);

            Assert.Same(comments, result);
        }
    }
}
