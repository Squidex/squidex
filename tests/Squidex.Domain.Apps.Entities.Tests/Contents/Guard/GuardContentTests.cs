// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
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
        private readonly ClaimsPrincipal user = new ClaimsPrincipal();
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));

        public GuardContentTests()
        {
            SetupSingleton(false);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_data_is_null()
        {
            var command = new CreateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanCreate(schema, contentWorkflow, command),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_singleton()
        {
            SetupSingleton(true);

            var command = new CreateContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(schema, contentWorkflow, command));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_singleton_and_id_is_schema_id()
        {
            SetupSingleton(true);

            var command = new CreateContent { Data = new NamedContentData(), ContentId = schema.Id };

            await GuardContent.CanCreate(schema, contentWorkflow, command);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_publish_not_allowed()
        {
            SetupCanCreatePublish(false);

            var command = new CreateContent { Data = new NamedContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(schema, contentWorkflow, command));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_publishing_allowed()
        {
            SetupCanCreatePublish(true);

            var command = new CreateContent { Data = new NamedContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(schema, contentWorkflow, command));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_data_is_not_null()
        {
            var command = new CreateContent { Data = new NamedContentData() };

            await GuardContent.CanCreate(schema, contentWorkflow, command);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new UpdateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanUpdate(content, contentWorkflow, command, false),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft, false);
            var command = new UpdateContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanUpdate(content, contentWorkflow, command, false));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new UpdateContent { Data = new NamedContentData() };

            await GuardContent.CanUpdate(content, contentWorkflow, command, false);
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new PatchContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanPatch(content, contentWorkflow, command, false),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft, false);
            var command = new PatchContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanPatch(content, contentWorkflow, command, false));
        }

        [Fact]
        public async Task CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft, false);
            var command = new PatchContent { Data = new NamedContentData() };

            await GuardContent.CanPatch(content, contentWorkflow, command, false);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_publishing_without_pending_changes()
        {
            var content = CreateContent(Status.Published, false);
            var command = new ChangeContentStatus { Status = Status.Published };

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command, true),
                new ValidationError("Content has no changes to publish.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_singleton()
        {
            SetupSingleton(true);

            var content = CreateContent(Status.Published, false);
            var command = new ChangeContentStatus { Status = Status.Draft };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command, false));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_publishing_with_pending_changes()
        {
            SetupSingleton(true);

            var content = CreateContent(Status.Published, true);
            var command = new ChangeContentStatus { Status = Status.Published };

            await GuardContent.CanChangeStatus(schema, content, contentWorkflow, command, true);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_due_date_in_past()
        {
            var content = CreateContent(Status.Draft, false);
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast, User = user };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, command.Status, user))
                .Returns(true);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command, false),
                new ValidationError("Due time must be in the future.", "DueTime"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var content = CreateContent(Status.Draft, false);
            var command = new ChangeContentStatus { Status = Status.Published, User = user };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, command.Status, user))
                .Returns(false);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command, false),
                new ValidationError("Cannot change status from Draft to Published.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_status_flow_valid()
        {
            var content = CreateContent(Status.Draft, false);
            var command = new ChangeContentStatus { Status = Status.Published, User = user };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, command.Status, user))
                .Returns(true);

            await GuardContent.CanChangeStatus(schema, content, contentWorkflow, command, false);
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
            var command = new DeleteContent();

            GuardContent.CanDelete(schema, command);
        }

        private void SetupCanUpdate(bool canUpdate)
        {
            A.CallTo(() => contentWorkflow.CanUpdateAsync(A<IContentEntity>.Ignored))
                .Returns(canUpdate);
        }

        private void SetupCanCreatePublish(bool canCreate)
        {
            A.CallTo(() => contentWorkflow.CanPublishOnCreateAsync(schema, A<NamedContentData>.Ignored, user))
                .Returns(canCreate);
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
