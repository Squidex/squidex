// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class DefaultContentWorkflowTests
    {
        private readonly DefaultContentWorkflow sut = new DefaultContentWorkflow();

        [Fact]
        public async Task Should_return_info_for_valid_status()
        {
            var info = await sut.GetInfoAsync(null!, Status.Draft);

            Assert.Equal(new StatusInfo(Status.Draft, StatusColors.Draft), info);
        }

        [Fact]
        public async Task Should_return_info_as_null_for_invalid_status()
        {
            var info = await sut.GetInfoAsync(null!, new Status("Invalid"));

            Assert.Null(info);
        }

        [Fact]
        public async Task Should_return_draft_as_initial_status()
        {
            var result = await sut.GetInitialStatusAsync(null!);

            Assert.Equal(Status.Draft, result);
        }

        [Fact]
        public async Task Should_allow_publish_on_create()
        {
            var result = await sut.CanPublishInitialAsync(null!, null);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_allow_if_transition_is_valid()
        {
            var content = new ContentEntity { Status = Status.Published };

            var result = await sut.CanMoveToAsync(null!, content.Status, Status.Draft, null!, null!);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_allow_if_transition_is_valid_for_content()
        {
            var content = new ContentEntity { Status = Status.Published };

            var result = await sut.CanMoveToAsync(content, content.Status, Status.Draft, null!);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_published()
        {
            var content = new ContentEntity { Status = Status.Published };

            var result = await sut.CanUpdateAsync(content, content.Status, null!);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_be_able_to_update_draft()
        {
            var content = new ContentEntity { Status = Status.Published };

            var result = await sut.CanUpdateAsync(content, content.Status, null!);

            Assert.True(result);
        }

        [Fact]
        public async Task Should_not_be_able_to_update_archived()
        {
            var content = new ContentEntity { Status = Status.Archived };

            var result = await sut.CanUpdateAsync(content, content.Status, null!);

            Assert.False(result);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_draft()
        {
            var content = new ContentEntity { Status = Status.Draft };

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived),
                new StatusInfo(Status.Published, StatusColors.Published)
            };

            var result = await sut.GetNextAsync(content, content.Status, null!);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_archived()
        {
            var content = new ContentEntity { Status = Status.Archived };

            var expected = new[]
            {
                new StatusInfo(Status.Draft, StatusColors.Draft)
            };

            var result = await sut.GetNextAsync(content, content.Status, null!);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_get_next_statuses_for_published()
        {
            var content = new ContentEntity { Status = Status.Published };

            var expected = new[]
            {
                new StatusInfo(Status.Archived, StatusColors.Archived),
                new StatusInfo(Status.Draft, StatusColors.Draft)
            };

            var result = await sut.GetNextAsync(content, content.Status, null!);

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

            var result = await sut.GetAllAsync(null!);

            result.Should().BeEquivalentTo(expected);
        }
    }
}
