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
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public class GuardContentTests : IClassFixture<TranslationsFixture>
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly ClaimsPrincipal user = Mocks.FrontendUser();
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));
        private readonly RefToken actor = new RefToken(RefTokenType.Subject, "123");

        [Fact]
        public async Task CanCreate_should_throw_exception_if_data_is_null()
        {
            var schema = CreateSchema(false);

            var command = new CreateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanCreate(command, contentWorkflow, schema),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var command = new CreateContent { Data = new ContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, contentWorkflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_singleton_and_id_is_schema_id()
        {
            var schema = CreateSchema(true);

            var command = new CreateContent { Data = new ContentData(), ContentId = schema.Id };

            await GuardContent.CanCreate(command, contentWorkflow, schema);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_publish_not_allowed()
        {
            var schema = CreateSchema(false);

            SetupCanCreatePublish(schema, false);

            var command = new CreateContent { Data = new ContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, contentWorkflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_publishing_allowed()
        {
            var schema = CreateSchema(false);

            SetupCanCreatePublish(schema, true);

            var command = new CreateContent { Data = new ContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, contentWorkflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_data_is_not_null()
        {
            var schema = CreateSchema(false);

            var command = new CreateContent { Data = new ContentData() };

            await GuardContent.CanCreate(command, contentWorkflow, schema);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new UpdateContent());

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanUpdate(command, content, contentWorkflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new UpdateContent { Data = new ContentData() });

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanUpdate(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new UpdateContent { Data = new ContentData() });

            await GuardContent.CanUpdate(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new PatchContent());

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanPatch(command, content, contentWorkflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new PatchContent { Data = new ContentData() });

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanPatch(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new PatchContent { Data = new ContentData() });

            await GuardContent.CanPatch(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new ChangeContentStatus { Status = Status.Draft });

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_due_date_in_past()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast });

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema),
                new ValidationError("Due time must be in the future.", "DueTime"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published });

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(false);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema),
                new ValidationError("Cannot change status from Draft to Published.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_referenced()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new ChangeContentStatus { Status = Status.Draft });

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.Published))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_singleton_is_published()
        {
            var schema = CreateSchema(true);

            var content = CreateDraftContent(Status.Draft);
            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published });

            await GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema);
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_status_flow_valid()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published });

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema);
        }

        [Fact]
        public void CreateDraft_should_throw_exception_if_not_published()
        {
            var content = CreateContent(Status.Draft);
            var command = CreateCommand(new CreateContentDraft());

            Assert.Throws<DomainException>(() => GuardContent.CanCreateDraft(command, content));
        }

        [Fact]
        public void CreateDraft_should_not_throw_exception()
        {
            var content = CreateContent(Status.Published);
            var command = CreateCommand(new CreateContentDraft());

            GuardContent.CanCreateDraft(command, content);
        }

        [Fact]
        public void CanDeleteDraft_should_throw_exception_if_no_draft_found()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new DeleteContentDraft());

            Assert.Throws<DomainException>(() => GuardContent.CanDeleteDraft(command, content));
        }

        [Fact]
        public void CanDeleteDraft_should_not_throw_exception()
        {
            var content = CreateDraftContent(Status.Draft);
            var command = CreateCommand(new DeleteContentDraft());

            GuardContent.CanDeleteDraft(command, content);
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new DeleteContent());

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_referenced()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new DeleteContent());

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.All))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        [Fact]
        public async Task CanDelete_should_not_throw_exception()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new DeleteContent());

            await GuardContent.CanDelete(command, content, contentRepository, schema);
        }

        [Fact]
        public void CheckPermission_should_not_throw_exception_if_content_is_from_current_user()
        {
            var content = CreateContent(status: Status.Published);
            var command = CreateCommand(new DeleteContent());

            GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete);
        }

        [Fact]
        public void CheckPermission_should_not_throw_exception_if_content_is_from_another_user_but_user_has_permission()
        {
            var permission = Permissions.ForApp(Permissions.AppContentsDelete, appId.Name, schemaId.Name).Id;

            var otherUser = Mocks.FrontendUser(permission: permission);
            var otherActor = new RefToken(RefTokenType.Subject, "456");

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new DeleteContent { Actor = otherActor, User = otherUser });

            GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete);
        }

        [Fact]
        public void CheckPermission_should_exception_if_content_is_from_another_user_and_user_has_no_permission()
        {
            var otherActor = new RefToken(RefTokenType.Subject, "456");

            var content = CreateContent(Status.Published);
            var command = CreateCommand(new DeleteContent { Actor = otherActor });

            Assert.Throws<DomainForbiddenException>(() => GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete));
        }

        private void SetupCanUpdate(bool canUpdate)
        {
            A.CallTo(() => contentWorkflow.CanUpdateAsync(A<IContentEntity>._, A<Status>._, user))
                .Returns(canUpdate);
        }

        private void SetupCanCreatePublish(ISchemaEntity schema, bool canCreate)
        {
            A.CallTo(() => contentWorkflow.CanPublishOnCreateAsync(schema, A<ContentData>._, user))
                .Returns(canCreate);
        }

        private ISchemaEntity CreateSchema(bool isSingleton)
        {
            return Mocks.Schema(appId, NamedId.Of(DomainId.NewGuid(), "my-schema"), new Schema("schema", isSingleton: isSingleton));
        }

        private T CreateCommand<T>(T command) where T : ContentCommand
        {
            if (command.Actor == null)
            {
                command.Actor = actor;
            }

            if (command.User == null)
            {
                command.User = user;
            }

            return command;
        }

        private IContentEntity CreateDraftContent(Status status)
        {
            return CreateContentCore(new ContentEntity { NewStatus = status });
        }

        private IContentEntity CreateContent(Status status)
        {
            return CreateContentCore(new ContentEntity { Status = status });
        }

        private IContentEntity CreateContentCore(ContentEntity content)
        {
            content.Id = DomainId.NewGuid();
            content.AppId = appId;
            content.Created = default;
            content.CreatedBy = actor;
            content.SchemaId = schemaId;

            return content;
        }
    }
}
