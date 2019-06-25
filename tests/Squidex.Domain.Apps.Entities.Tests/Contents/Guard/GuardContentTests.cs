// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Guards;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Guard
{
    public class GuardContentTests
    {
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));

        [Fact]
        public void CanCreate_should_throw_exception_if_data_is_null()
        {
            SetupSingleton(false);

            var command = new CreateContent();

            ValidationAssert.Throws(() => GuardContent.CanCreate(schema, command),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_singleton()
        {
            SetupSingleton(true);

            var command = new CreateContent { Data = new NamedContentData() };

            Assert.Throws<DomainException>(() => GuardContent.CanCreate(schema, command));
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_singleton_and_id_is_schema_id()
        {
            SetupSingleton(true);

            var command = new CreateContent { Data = new NamedContentData(), ContentId = schema.Id };

            GuardContent.CanCreate(schema, command);
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupSingleton(false);

            var command = new CreateContent { Data = new NamedContentData() };

            GuardContent.CanCreate(schema, command);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_data_is_null()
        {
            SetupSingleton(false);
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new UpdateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanUpdate(content, contentWorkflow, command),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_workflow_blocks_it()
        {
            SetupSingleton(false);
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft, false);
            var command = new UpdateContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanUpdate(content, contentWorkflow, command));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupSingleton(false);
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new UpdateContent { Data = new NamedContentData() };

            await GuardContent.CanUpdate(content, contentWorkflow, command);
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_data_is_null()
        {
            SetupSingleton(false);
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new PatchContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanPatch(content, contentWorkflow, command),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_workflow_blocks_it()
        {
            SetupSingleton(false);
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft, false);
            var command = new PatchContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanPatch(content, contentWorkflow, command));
        }

        [Fact]
        public async Task CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            SetupSingleton(false);
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new PatchContent { Data = new NamedContentData() };

            await GuardContent.CanPatch(content, contentWorkflow, command);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_publishing_without_pending_changes()
        {
            SetupSingleton(false);

            var content = CreateContent(Status.Published, false);
            var command = new ChangeContentStatus { Status = Status.Published };

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command),
                new ValidationError("Content has no changes to publish.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_singleton()
        {
            SetupSingleton(true);

            var content = CreateContent(Status.Published, false);
            var command = new ChangeContentStatus { Status = Status.Draft };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_publishing_with_pending_changes()
        {
            SetupSingleton(true);

            var content = CreateContent(Status.Published, true);
            var command = new ChangeContentStatus { Status = Status.Published };

            await GuardContent.CanChangeStatus(schema, content, contentWorkflow, command);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_due_date_in_past()
        {
            SetupSingleton(false);

            var content = CreateContent(Status.Draft, false);
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, command.Status))
                .Returns(true);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command),
                new ValidationError("Due time must be in the future.", "DueTime"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_status_flow_not_valid()
        {
            SetupSingleton(false);

            var content = CreateContent(Status.Draft, false);
            var command = new ChangeContentStatus { Status = Status.Published };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, command.Status))
                .Returns(false);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command),
                new ValidationError("Cannot change status from Draft to Published.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_status_flow_valid()
        {
            SetupSingleton(false);

            var content = CreateContent(Status.Draft, false);
            var command = new ChangeContentStatus { Status = Status.Published };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, command.Status))
                .Returns(true);

            await GuardContent.CanChangeStatus(schema, content, contentWorkflow, command);
        }

        [Fact]
        public void CanDiscardChanges_should_throw_exception_if_pending()
        {
            var command = new DiscardChanges();

            Assert.Throws<DomainException>(() => GuardContent.CanDiscardChanges(false, command));
        }

        [Fact]
        public void CanDiscardChanges_should_not_throw_exception_if_pending()
        {
            SetupSingleton(false);

            var command = new DiscardChanges();

            GuardContent.CanDiscardChanges(true, command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_singleton()
        {
            SetupSingleton(true);

            var command = new DeleteContent();

            Assert.Throws<DomainException>(() => GuardContent.CanDelete(schema, command));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception()
        {
            SetupSingleton(false);

            var command = new DeleteContent();

            GuardContent.CanDelete(schema, command);
        }

        private void SetupCanUpdate(bool canUpdate)
        {
            A.CallTo(() => contentWorkflow.CanUpdateAsync(A<IContentEntity>.Ignored))
                .Returns(canUpdate);
        }

        private void SetupSingleton(bool isSingleton)
        {
            A.CallTo(() => schema.SchemaDef)
                .Returns(new Schema("schema", isSingleton: isSingleton));
        }

        private IContentEntity CreateContent(Status status, bool isPending)
        {
            return new ContentEntity { Status = status, IsPending = isPending };
        }
    }
}
