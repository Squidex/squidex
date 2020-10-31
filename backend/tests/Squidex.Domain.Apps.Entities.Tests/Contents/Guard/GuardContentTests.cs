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
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Guards;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Guard
{
    public class GuardContentTests : IClassFixture<TranslationsFixture>
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly ClaimsPrincipal user = Mocks.FrontendUser();
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));

        [Fact]
        public async Task CanCreate_should_throw_exception_if_data_is_null()
        {
            var schema = CreateSchema(false);

            var command = new CreateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanCreate(schema, contentWorkflow, command),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var command = new CreateContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(schema, contentWorkflow, command));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_singleton_and_id_is_schema_id()
        {
            var schema = CreateSchema(true);

            var command = new CreateContent { Data = new NamedContentData(), ContentId = schema.Id };

            await GuardContent.CanCreate(schema, contentWorkflow, command);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_publish_not_allowed()
        {
            var schema = CreateSchema(false);

            SetupCanCreatePublish(schema, false);

            var command = new CreateContent { Data = new NamedContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(schema, contentWorkflow, command));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_publishing_allowed()
        {
            var schema = CreateSchema(false);

            SetupCanCreatePublish(schema, true);

            var command = new CreateContent { Data = new NamedContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(schema, contentWorkflow, command));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_data_is_not_null()
        {
            var schema = CreateSchema(false);

            var command = new CreateContent { Data = new NamedContentData() };

            await GuardContent.CanCreate(schema, contentWorkflow, command);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = new UpdateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanUpdate(content, contentWorkflow, command),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft);
            var command = new UpdateContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanUpdate(content, contentWorkflow, command));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = new UpdateContent { Data = new NamedContentData(), User = user };

            await GuardContent.CanUpdate(content, contentWorkflow, command);
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = new PatchContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanPatch(content, contentWorkflow, command),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft);
            var command = new PatchContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanPatch(content, contentWorkflow, command));
        }

        [Fact]
        public async Task CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = new PatchContent { Data = new NamedContentData(), User = user };

            await GuardContent.CanPatch(content, contentWorkflow, command);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = new ChangeContentStatus { Status = Status.Draft };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_due_date_in_past()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast, User = user };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command),
                new ValidationError("Due time must be in the future.", "DueTime"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = new ChangeContentStatus { Status = Status.Published, User = user };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(false);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(schema, content, contentWorkflow, command),
                new ValidationError("Cannot change status from Draft to Published.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_singleton_is_published()
        {
            var schema = CreateSchema(true);

            var content = CreateDraftContent(Status.Draft);
            var command = new ChangeContentStatus { Status = Status.Published };

            await GuardContent.CanChangeStatus(schema, content, contentWorkflow, command);
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_status_flow_valid()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = new ChangeContentStatus { Status = Status.Published, User = user };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await GuardContent.CanChangeStatus(schema, content, contentWorkflow, command);
        }

        [Fact]
        public void CreateDraft_should_throw_exception_if_not_published()
        {
            CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = new CreateContentDraft();

            Assert.Throws<DomainException>(() => GuardContent.CanCreateDraft(command, content));
        }

        [Fact]
        public void CreateDraft_should_not_throw_exception()
        {
            var content = CreateContent(Status.Published);
            var command = new CreateContentDraft();

            GuardContent.CanCreateDraft(command, content);
        }

        [Fact]
        public void CanDeleteDraft_should_throw_exception_if_no_draft_found()
        {
            CreateSchema(false);

            var content = CreateContent(Status.Published);
            var command = new DeleteContentDraft();

            Assert.Throws<DomainException>(() => GuardContent.CanDeleteDraft(command, content));
        }

        [Fact]
        public void CanDeleteDraft_should_not_throw_exception()
        {
            var content = CreateDraftContent(Status.Draft);
            var command = new DeleteContentDraft();

            GuardContent.CanDeleteDraft(command, content);
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = new DeleteContent();

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(schema, content, contentRepository, command));
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_referenced()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = new DeleteContent();

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(schema, content, contentRepository, command));
        }

        [Fact]
        public async Task CanDelete_should_not_throw_exception()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Published);
            var command = new DeleteContent();

            await GuardContent.CanDelete(schema, content, contentRepository, command);
        }

        private void SetupCanUpdate(bool canUpdate)
        {
            A.CallTo(() => contentWorkflow.CanUpdateAsync(A<IContentEntity>._, A<Status>._, user))
                .Returns(canUpdate);
        }

        private void SetupCanCreatePublish(ISchemaEntity schema, bool canCreate)
        {
            A.CallTo(() => contentWorkflow.CanPublishOnCreateAsync(schema, A<NamedContentData>._, user))
                .Returns(canCreate);
        }

        private ISchemaEntity CreateSchema(bool isSingleton)
        {
            return Mocks.Schema(appId, NamedId.Of(DomainId.NewGuid(), "my-schema"), new Schema("schema", isSingleton: isSingleton));
        }

        private ContentState CreateDraftContent(Status status)
        {
            return new ContentState
            {
                Id = DomainId.NewGuid(),
                NewVersion = new ContentVersion(status, new NamedContentData()),
                AppId = appId
            };
        }

        private ContentState CreateContent(Status status)
        {
            return new ContentState
            {
                Id = DomainId.NewGuid(),
                CurrentVersion = new ContentVersion(status, new NamedContentData()),
                AppId = appId
            };
        }
    }
}
