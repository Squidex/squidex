// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class DefaultContentWorkflowTests
    {
        private readonly DefaultContentWorkflow sut = new DefaultContentWorkflow();

        [Fact]
        public async Task Should_draft_as_initial_status()
        {
            var result = await sut.GetInitialStatusAsync(null);

            Assert.Equal(Status.Draft, result);
        }

        [Fact]
        public async Task Should_check_is_valid_next()
        {
            var entity = CreateContent(Status.Published);

            var result = await sut.IsValidNextStatus(entity, Status.Draft);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_always_be_able_to_update()
        {
            var entity = CreateContent(Status.Published);

            var result = await sut.CanUpdateAsync(entity);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_draft()
        {
            var content = CreateContent(Status.Draft);

            var expected = new[] { Status.Archived, Status.Published };

            var result = await sut.GetNextsAsync(content);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_archived()
        {
            var content = CreateContent(Status.Archived);

            var expected = new[] { Status.Draft };

            var result = await sut.GetNextsAsync(content);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_published()
        {
            var content = CreateContent(Status.Published);

            var expected = new[] { Status.Draft, Status.Archived };

            var result = await sut.GetNextsAsync(content);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Should_return_all_statuses()
        {
            var expected = new[] { Status.Archived, Status.Draft, Status.Published };

            var result = await sut.GetAllAsync(null);

            Assert.Equal(expected, result);
        }

        private IContentEntity CreateContent(Status status)
        {
            var content = A.Fake<IContentEntity>();

            A.CallTo(() => content.Status).Returns(status);

            return content;
        }
    }
}
