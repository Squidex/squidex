// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
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
            var expected = new StatusInfo(Status.Draft, StatusColors.Draft);

            var result = await sut.GetInitialStatusAsync(null);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_check_is_valid_next()
        {
            var entity = CreateContent(Status.Published);

            var result = await sut.CanMoveToAsync(entity, Status.Draft);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_published()
        {
            var entity = CreateContent(Status.Published);

            var result = await sut.CanUpdateAsync(entity);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_draft()
        {
            var entity = CreateContent(Status.Published);

            var result = await sut.CanUpdateAsync(entity);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_be_able_to_update_archived()
        {
            var entity = CreateContent(Status.Archived);

            var result = await sut.CanUpdateAsync(entity);

            Assert.False(result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_draft()
        {
            var content = CreateContent(Status.Draft);

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived),
                new StatusInfo(Status.Published, StatusColors.Published)
            };

            var result = await sut.GetNextsAsync(content);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_archived()
        {
            var content = CreateContent(Status.Archived);

            var expected = new[]
            {
                new StatusInfo(Status.Draft, StatusColors.Draft)
            };

            var result = await sut.GetNextsAsync(content);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_published()
        {
            var content = CreateContent(Status.Published);

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived),
                new StatusInfo(Status.Draft, StatusColors.Draft)
            };

            var result = await sut.GetNextsAsync(content);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_return_all_statuses()
        {
            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived),
                new StatusInfo(Status.Draft, StatusColors.Draft),
                new StatusInfo(Status.Published, StatusColors.Published)
            };

            var result = await sut.GetAllAsync(null);

            result.Should().BeEquivalentTo(expected);
        }

        private IContentEntity CreateContent(Status status)
        {
            var content = A.Fake<IContentEntity>();

            A.CallTo(() => content.Status).Returns(status);

            return content;
        }
    }
}
