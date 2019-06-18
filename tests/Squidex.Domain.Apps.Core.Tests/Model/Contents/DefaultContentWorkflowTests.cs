// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Xunit;

namespace Squidex.Domain.Apps.Core.Model.Contents
{
    public class DefaultContentWorkflowTests
    {
        private static readonly DefaultContentWorkflow Workflow = new DefaultContentWorkflow();

        [Fact]
        public void Get_initial_status_async_tests()
        {
            var entity = new SchemaState();

            var result = Workflow.GetInitialStatusAsync(entity);

            Assert.IsType<Task<Status2>>(result);
            Assert.Equal("Draft", result.Result.Name);
        }

        [Fact]
        public void Is_valid_next_status_tests()
        {
            var entity = new ContentEntity();

            var status = new Status2("Draft");

            var result = Workflow.IsValidNextStatus(entity, status);

            Assert.IsType<Task<bool>>(result);
            Assert.True(result.Result);
        }

        [Fact]
        public void Can_update_async_tests()
        {
            var entity = new ContentEntity();

            var result = Workflow.CanUpdateAsync(entity);

            Assert.IsType<Task<bool>>(result);
            Assert.True(result.Result);
        }

        [Fact]
        public void Get_nexts_async_tests()
        {
            var draftContent = new ContentEntity() { Status = Status.Draft };
            var archivedContent = new ContentEntity() { Status = Status.Archived };
            var publishedContent = new ContentEntity() { Status = Status.Published };

            Status2[] draftExpected = { new Status2("Published"), new Status2("Archived") };
            Status2[] archivedExpected = { new Status2("Draft") };
            Status2[] publishedExpected = { new Status2("Draft"), new Status2("Archived") };

            var draftResult = Workflow.GetNextsAsync(draftContent);
            var archivedResult = Workflow.GetNextsAsync(archivedContent);
            var publishedResult = Workflow.GetNextsAsync(publishedContent);

            Assert.IsType<Task<Status2[]>>(draftResult);
            Assert.IsType<Task<Status2[]>>(archivedResult);
            Assert.IsType<Task<Status2[]>>(publishedResult);

            Assert.Equal(draftExpected, draftResult.Result);
            Assert.Equal(archivedExpected, archivedResult.Result);
            Assert.Equal(publishedExpected, publishedResult.Result);
        }

        [Fact]
        public void Get_all_async_tests()
        {
            var entity = new SchemaState();

            Status2[] expected = { new Status2("Draft"), new Status2("Archived"), new Status2("Published") };

            var result = Workflow.GetAllAsync(entity);

            Assert.IsType<Task<Status2[]>>(result);
            Assert.Equal(expected, result.Result);
        }
    }
}
