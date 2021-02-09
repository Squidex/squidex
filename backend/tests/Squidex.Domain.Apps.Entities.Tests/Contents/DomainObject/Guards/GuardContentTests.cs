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
        private readonly IContentWorkflow workflow = A.Fake<IContentWorkflow>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly ISchemaEntity schema;
        private readonly ISchemaEntity singleton;
        private readonly ClaimsPrincipal user = Mocks.FrontendUser();
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));
        private readonly RefToken actor = RefToken.User("123");

        public GuardContentTests()
        {
            schema =
                Mocks.Schema(appId, schemaId, new Schema(schemaId.Name));

            singleton =
                Mocks.Schema(appId, schemaId, new Schema(schemaId.Name, isSingleton: true));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_data_is_null()
        {
            var command = new CreateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanCreate(command, workflow, schema),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_singleton()
        {
            var command = new CreateContent { Data = new ContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, workflow, singleton));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_singleton_and_id_is_schema_id()
        {
            var command = new CreateContent { Data = new ContentData(), ContentId = schema.Id };

            await GuardContent.CanCreate(command, workflow, schema);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_if_publishing_not_allowed()
        {
            SetupCanCreatePublish(false);

            var command = new CreateContent { Data = new ContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, workflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_publishing_allowed()
        {
            SetupCanCreatePublish(true);

            var command = new CreateContent { Data = new ContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, workflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_data_is_not_null()
        {
            var command = new CreateContent { Data = new ContentData() };

            await GuardContent.CanCreate(command, workflow, schema);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_data_is_null()
        {
            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new UpdateContent());

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanUpdate(command, content, workflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new UpdateContent { Data = new ContentData() });

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanUpdate(command, content, workflow));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new UpdateContent { Data = new ContentData() });

            await GuardContent.CanUpdate(command, content, workflow);
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new PatchContent());

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanPatch(command, content, workflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false);

            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new PatchContent { Data = new ContentData() });

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanPatch(command, content, workflow));
        }

        [Fact]
        public async Task CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true);

            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new PatchContent { Data = new ContentData() });

            await GuardContent.CanPatch(command, content, workflow);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_singleton()
        {
            var content = CreateContent(Status.Published);

            var command = CreateCommand(new ChangeContentStatus { Status = Status.Draft });

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(command, content, workflow, contentRepository, singleton));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_due_date_in_past()
        {
            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast });

            A.CallTo(() => workflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(command, content, workflow, contentRepository, schema),
                new ValidationError("Due time must be in the future.", "DueTime"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published });

            A.CallTo(() => workflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(false);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(command, content, workflow, contentRepository, schema),
                new ValidationError("Cannot change status from Draft to Published.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_referenced()
        {
            var content = CreateContent(Status.Published);

            var command = CreateCommand(new ChangeContentStatus { Status = Status.Draft });

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.Published))
                .Returns(true);

            await Assert.ThrowsAsync<ValidationException>(() => GuardContent.CanChangeStatus(command, content, workflow, contentRepository, schema));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_singleton_is_published()
        {
            var content = CreateDraftContent(Status.Draft);

            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published });

            await GuardContent.CanChangeStatus(command, content, workflow, contentRepository, singleton);
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_status_flow_valid()
        {
            var content = CreateContent(Status.Draft);

            var command = CreateCommand(new ChangeContentStatus { Status = Status.Published });

            A.CallTo(() => workflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await GuardContent.CanChangeStatus(command, content, workflow, contentRepository, schema);
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
            var content = CreateContent(Status.Published);

            var command = CreateCommand(new DeleteContent());

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, singleton));
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_referenced()
        {
            var content = CreateContent(Status.Published);

            var command = CreateCommand(new DeleteContent { CheckReferrers = true });

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.All))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        [Fact]
        public async Task CanDelete_should_not_throw_exception()
        {
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
        public void CheckPermission_should_not_throw_exception_if_user_is_null()
        {
            var content = CreateContent(Status.Published);

            var commandActor = RefToken.User("456");
            var command = CreateCommand(new DeleteContent { Actor = commandActor });

            command.User = null;

            GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete);
        }

        [Fact]
        public void CheckPermission_should_not_throw_exception_if_content_is_from_another_user_but_user_has_permission()
        {
            var content = CreateContent(Status.Published);

            var permission = Permissions.ForApp(Permissions.AppContentsDelete, appId.Name, schemaId.Name).Id;

            var commandUser = Mocks.FrontendUser(permission: permission);
            var commandActor = RefToken.User("456");
            var command = CreateCommand(new DeleteContent { Actor = commandActor, User = commandUser });

            GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete);
        }

        [Fact]
        public void CheckPermission_should_exception_if_content_is_from_another_user_and_user_has_no_permission()
        {
            var content = CreateContent(Status.Published);

            var commandActor = RefToken.User("456");
            var command = CreateCommand(new DeleteContent { Actor = commandActor });

            Assert.Throws<DomainForbiddenException>(() => GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete));
        }

        private void SetupCanUpdate(bool canUpdate)
        {
            A.CallTo(() => workflow.CanUpdateAsync(A<IContentEntity>._, A<Status>._, user))
                .Returns(canUpdate);
        }

        private void SetupCanCreatePublish(bool canCreate)
        {
            A.CallTo(() => workflow.CanPublishOnCreateAsync(schema, A<ContentData>._, user))
                .Returns(canCreate);
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
