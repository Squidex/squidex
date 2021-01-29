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
        private readonly ClaimsPrincipal user = Mocks.FrontendUser();
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));
        private readonly RefToken refToken = new RefToken("string", "string");

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

            var command = new CreateContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, contentWorkflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_singleton_and_id_is_schema_id()
        {
            var schema = CreateSchema(true);

            var command = new CreateContent { Data = new NamedContentData(), ContentId = schema.Id };

            await GuardContent.CanCreate(command, contentWorkflow, schema);
        }

        [Fact]
        public async Task CanCreate_should_throw_exception_publish_not_allowed()
        {
            var schema = CreateSchema(false);

            SetupCanCreatePublish(schema, false);

            var command = new CreateContent { Data = new NamedContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, contentWorkflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_publishing_allowed()
        {
            var schema = CreateSchema(false);

            SetupCanCreatePublish(schema, true);

            var command = new CreateContent { Data = new NamedContentData(), Publish = true };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanCreate(command, contentWorkflow, schema));
        }

        [Fact]
        public async Task CanCreate_should_not_throw_exception_if_data_is_not_null()
        {
            var schema = CreateSchema(false);

            var command = new CreateContent { Data = new NamedContentData() };

            await GuardContent.CanCreate(command, contentWorkflow, schema);
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true, user);
            var schema = CreateSchema(false);
            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new UpdateContent { Actor = refToken };

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanUpdate(command, content, contentWorkflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false, user);
            var schema = CreateSchema(false);
            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new UpdateContent { Data = new NamedContentData(), Actor = refToken };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanUpdate(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true, user);
            var schema = CreateSchema(false);
            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new UpdateContent { Data = new NamedContentData(), User = user, Actor = refToken };

            await GuardContent.CanUpdate(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true, user);
            var schema = CreateSchema(false);
            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new PatchContent { Actor = refToken };

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanPatch(command, content, contentWorkflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false, user);
            var schema = CreateSchema(false);
            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new PatchContent { Data = new NamedContentData(), Actor = refToken };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanPatch(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true, user);
            var schema = CreateSchema(false);
            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new PatchContent { Data = new NamedContentData(), User = user, Actor = refToken };

            await GuardContent.CanPatch(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published, schema: schema, createdBy: refToken);
            var command = new ChangeContentStatus { Status = Status.Draft, Actor = refToken };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_due_date_in_past()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast, User = user, Actor = refToken };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema),
                new ValidationError("Due time must be in the future.", "DueTime"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_status_flow_not_valid()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken );
            var command = new ChangeContentStatus { Status = Status.Published, User = user, Actor = refToken };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(false);

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema),
                new ValidationError("Cannot change status from Draft to Published.", "Status"));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_referenced()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published, createdBy: refToken, schema: schema);
            var command = new ChangeContentStatus { Status = Status.Draft, User = user, Actor = refToken };

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.Published))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_singleton_is_published()
        {
            var schema = CreateSchema(true);

            var content = CreateDraftContent(Status.Draft, createdBy: refToken);
            var command = new ChangeContentStatus { Status = Status.Published, User = user, Actor = refToken };

            await GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema);
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_status_flow_valid()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft, schema: schema, createdBy: refToken);
            var command = new ChangeContentStatus { Status = Status.Published, User = user, Actor = refToken };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema);
        }

        [Fact]
        public void CreateDraft_should_throw_exception_if_not_published()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft, schema: schema);
            var command = new CreateContentDraft();

            Assert.Throws<DomainException>(() => GuardContent.CanCreateDraft(command, content));
        }

        [Fact]
        public void CreateDraft_should_not_throw_exception()
        {
            var schema = CreateSchema(false);
            var content = CreateContent(Status.Published, schema: schema);
            var command = new CreateContentDraft();

            GuardContent.CanCreateDraft(command, content);
        }

        [Fact]
        public void CanDeleteDraft_should_throw_exception_if_no_draft_found()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Published, schema: schema, createdBy: refToken);
            var command = new DeleteContentDraft { Actor = refToken };

            Assert.Throws<DomainException>(() => GuardContent.CanDeleteDraft(command, content));
        }

        [Fact]
        public void CanDeleteDraft_should_not_throw_exception()
        {
            var content = CreateDraftContent(Status.Draft, refToken);
            var command = new DeleteContentDraft { Actor = refToken };

            GuardContent.CanDeleteDraft(command, content);
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published, schema: schema, createdBy: refToken);
            var command = new DeleteContent { Actor = refToken };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_referenced()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published, schema: schema, createdBy: refToken);
            var command = new DeleteContent { Actor = refToken };

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.All))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        [Fact]
        public async Task CanDelete_should_not_throw_exception()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Published, schema: schema, createdBy: refToken);
            var command = new DeleteContent { Actor = refToken };

            await GuardContent.CanDelete(command, content, contentRepository, schema);
        }

        [Fact]
        public void CheckPermission_should_if_content_has_createdby_should_return()
        {
            var schema = CreateSchema(false);
            var content = CreateContent(status: Status.Published, createdBy: refToken, schema: schema);
            var command = new DeleteContent { Actor = refToken };

            GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete);
        }

        [Fact]
        public void CheckPermission_if_user_has_permission_should_not_throw_exception()
        {
            var schema = CreateSchema(false);
            var wrongRefToken = new RefToken("stringwrong", "stringwrong");
            var content = CreateContent(status: Status.Published, createdBy: wrongRefToken, schema: schema);
            var command = CreateContentDeleteCommand(content);

            GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete);
        }

        [Fact]
        public void CheckPermission_if_user_has_not_permission_should_throw_exception()
        {
            var schema = CreateSchema(false);
            var wrongRefToken = new RefToken("stringwrong", "stringwrong");
            var content = CreateContent(status: Status.Published, createdBy: wrongRefToken, schema: schema);
            var command = new DeleteContent { Actor = refToken, User = user };

            Assert.Throws<DomainForbiddenException>(() => GuardContent.CheckPermission(content, command, Permissions.AppContentsDelete));
        }

        private void SetupCanUpdate(bool canUpdate, ClaimsPrincipal userMock)
        {
            A.CallTo(() => contentWorkflow.CanUpdateAsync(A<IContentEntity>._, A<Status>._, userMock))
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

        private IContentEntity CreateDraftContent(Status status, RefToken? createdBy)
        {
            return new ContentEntity
            {
                Id = DomainId.NewGuid(),
                NewStatus = status,
                AppId = appId,
                CreatedBy = createdBy
            };
        }

        private IContentEntity CreateContent(Status status, RefToken? createdBy = null, ISchemaEntity? schema = null)
        {
            return new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Status = status,
                AppId = appId,
                CreatedBy = createdBy,
                SchemaId = schema.AppId
            };
        }

        private DeleteContent CreateContentDeleteCommand(IContentEntity content)
        {
            return new DeleteContent
            {
                Actor = content.CreatedBy,
                User = Mocks.FrontendUser(permission: Permissions.AppContentsDelete)
            };
        }
    }
}
