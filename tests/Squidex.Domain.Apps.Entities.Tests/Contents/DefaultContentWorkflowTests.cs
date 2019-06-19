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

            Assert.Equal(new Status2("Draft"), result);
        }

        [Fact]
        public async Task Should_check_is_valid_next()
        {
            var entity = CreateMockContentEntity(Status.Draft);

            var result = await sut.IsValidNextStatus(entity, new Status2("Draft"));

            Assert.True(result);
        }

        [Fact]
        public async Task Should_always_be_able_to_update()
        {
            var entity = CreateMockContentEntity(Status.Draft);

            var result = await sut.CanUpdateAsync(entity);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_draft()
        {
            var content = CreateMockContentEntity(Status.Draft);

            var expected = new[] { new Status2("Published"), new Status2("Archived") };

            var result = await sut.GetNextsAsync(content);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_archived()
        {
            var content = CreateMockContentEntity(Status.Archived);

            var expected = new[] { new Status2("Draft") };

            var result = await sut.GetNextsAsync(content);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_published()
        {
            var content = CreateMockContentEntity(Status.Published);

            var expected = new[] { new Status2("Draft"), new Status2("Archived") };

            var result = await sut.GetNextsAsync(content);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Should_return_all_statuses()
        {
            var expected = new[] { new Status2("Draft"), new Status2("Archived"), new Status2("Published") };

            var result = await sut.GetAllAsync(null);

            Assert.Equal(expected, result);
        }

        private IContentEntity CreateMockContentEntity(Status status)
        {
            var content = A.Fake<IContentEntity>();

            A.CallTo(() => content.Status).Returns(status);

            return content;
        }
    }
}
