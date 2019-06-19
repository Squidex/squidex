// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class DefaultContentWorkflowTests
    {
        private static readonly DefaultContentWorkflow Sut = new DefaultContentWorkflow();

        [Fact]
        public async Task Should_draft_as_initial_status_async_tests()
        {
            var result = await Sut.GetInitialStatusAsync(null);

            Assert.IsType<Status2>(result);
            Assert.Equal("Draft", result.Name);
        }

        [Fact]
        public async Task Should_check_is_valid_next_status_tests()
        {
            var entity = CreateMockContentEntity(Status.Draft);

            var status = new Status2("Draft");

            var result = await Sut.IsValidNextStatus(entity, status);

            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Fact]
        public async Task Should_update_async_tests()
        {
            var entity = CreateMockContentEntity(Status.Draft);

            var result = await Sut.CanUpdateAsync(entity);

            Assert.IsType<bool>(result);
            Assert.True(result);
        }

        [Fact]
        public async Task Should_get_nexts_async_tests()
        {
            var draftContent = CreateMockContentEntity(Status.Draft);
            var archivedContent = CreateMockContentEntity(Status.Archived);
            var publishedContent = CreateMockContentEntity(Status.Published);

            var draftExpected = new[] { new Status2("Published"), new Status2("Archived") };
            var archivedExpected = new[] { new Status2("Draft") };
            var publishedExpected = new[] { new Status2("Draft"), new Status2("Archived") };

            var draftResult = await Sut.GetNextsAsync(draftContent);
            var archivedResult = await Sut.GetNextsAsync(archivedContent);
            var publishedResult = await Sut.GetNextsAsync(publishedContent);

            Assert.IsType<Status2[]>(draftResult);
            Assert.IsType<Status2[]>(archivedResult);
            Assert.IsType<Status2[]>(publishedResult);

            Assert.Equal(draftExpected, draftResult);
            Assert.Equal(archivedExpected, archivedResult);
            Assert.Equal(publishedExpected, publishedResult);
        }

        [Fact]
        public async Task Should_get_all_async_tests()
        {
            var expected = new[] { new Status2("Draft"), new Status2("Archived"), new Status2("Published") };

            var result = await Sut.GetAllAsync(null);

            Assert.IsType<Status2[]>(result);
            Assert.Equal(expected, result);
        }

        private IContentEntity CreateMockContentEntity(Status status)
        {
            var content = A.Fake<IContentEntity>();

            A.CallTo(() => content.Id).Returns(default(Guid));
            A.CallTo(() => content.Data).Returns(null);
            A.CallTo(() => content.DataDraft).Returns(null);
            A.CallTo(() => content.SchemaId).Returns(null);
            A.CallTo(() => content.Status).Returns(status);

            return content;
        }
    }
}
