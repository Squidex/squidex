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
        private readonly ClaimsPrincipal userWithDeletePermission = Mocks.FrontendUser(permission: Permissions.AppContentsDelete);
        private readonly ClaimsPrincipal userWithStatusChangePermission = Mocks.FrontendUser(permission: Permissions.AppContentsChangeStatus);
        private readonly ClaimsPrincipal userWithUpdatePermission = Mocks.FrontendUser(permission: Permissions.AppContentsUpdate);
        private readonly ClaimsPrincipal userWithPatchPermission = Mocks.FrontendUser(permission: Permissions.AppContentsPatch);
        private readonly ClaimsPrincipal userWithDeleteDraftPermission = Mocks.FrontendUser(permission: Permissions.AppContentsDeleteDraft);
        private readonly Instant dueTimeInPast = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(1));

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

            var content = CreateContent(Status.Draft);
            var command = new UpdateContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanUpdate(command, content, contentWorkflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanUpdate_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false, user);

            var content = CreateContent(Status.Draft);
            var command = new UpdateContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanUpdate(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanUpdate_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true, user);

            var content = CreateContent(Status.Draft);
            var command = new UpdateContent { Data = new NamedContentData(), User = user };

            await GuardContent.CanUpdate(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanUpdateOwn_should_not_throw_exception_if_has_permission()
        {
            SetupCanUpdate(true, userWithUpdatePermission);
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Draft, refToken: refToken, schema: schema);
            var command = UpdateContentCommand(content);

            await GuardContent.CanUpdate(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanUpdateOwn_should_throw_exception_if_has_not_permission()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Draft, refToken: refToken, schema: schema);
            var command = new UpdateContent { User = user };

            await Assert.ThrowsAsync<DomainForbiddenException>(() => GuardContent.CanUpdate(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_data_is_null()
        {
            SetupCanUpdate(true, user);

            var content = CreateContent(Status.Draft);
            var command = new PatchContent();

            await ValidationAssert.ThrowsAsync(() => GuardContent.CanPatch(command, content, contentWorkflow),
                new ValidationError("Data is required.", "Data"));
        }

        [Fact]
        public async Task CanPatch_should_throw_exception_if_workflow_blocks_it()
        {
            SetupCanUpdate(false, user);

            var content = CreateContent(Status.Draft);
            var command = new PatchContent { Data = new NamedContentData() };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanPatch(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanPatch_should_not_throw_exception_if_data_is_not_null()
        {
            SetupCanUpdate(true, user);

            var content = CreateContent(Status.Draft);
            var command = new PatchContent { Data = new NamedContentData(), User = user };

            await GuardContent.CanPatch(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanPatchOwn_should_not_throw_exception_if_has_permission()
        {
            SetupCanUpdate(true, userWithPatchPermission);
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Draft, refToken: refToken, schema: schema);
            var command = PatchContentCommand(content);

            await GuardContent.CanPatch(command, content, contentWorkflow);
        }

        [Fact]
        public async Task CanPatchOwn_should_throw_exception_if_has_not_permission()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Draft, refToken: refToken, schema: schema);
            var command = new PatchContent { User = user };

            await Assert.ThrowsAsync<DomainForbiddenException>(() => GuardContent.CanPatch(command, content, contentWorkflow));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = new ChangeContentStatus { Status = Status.Draft };

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema));
        }

        [Fact]
        public async Task CanChangeStatus_should_throw_exception_if_due_date_in_past()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = new ChangeContentStatus { Status = Status.Published, DueTime = dueTimeInPast, User = user };

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
            var command = new ChangeContentStatus { Status = Status.Published, User = user };

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
            var command = new ChangeContentStatus { Status = Status.Draft, User = user };

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.Published))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema));
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_singleton_is_published()
        {
            var schema = CreateSchema(true);

            var content = CreateDraftContent(Status.Draft);
            var command = new ChangeContentStatus { Status = Status.Published };

            await GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema);
        }

        [Fact]
        public async Task CanChangeStatus_should_not_throw_exception_if_status_flow_valid()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Draft);
            var command = new ChangeContentStatus { Status = Status.Published, User = user };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, user))
                .Returns(true);

            await GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema);
        }

        [Fact]
        public async Task CanChangeOwnStatus_should_not_throw_exception_if_has_permission()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Draft, refToken: refToken, schema: schema);
            var command = ChangeContentSatusCommand(content);

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, userWithStatusChangePermission))
                .Returns(true);

            await GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema);
        }

        [Fact]
        public async Task CanChangeOwnStatus_should_throw_exception_if_has_no_permission()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Draft, refToken: refToken, schema: schema);
            var command = new ChangeContentStatus { User = user, Status = Status.Published };

            A.CallTo(() => contentWorkflow.CanMoveToAsync(content, content.Status, command.Status, userWithStatusChangePermission))
                .Returns(true);

            await Assert.ThrowsAsync<DomainForbiddenException>(() => GuardContent.CanChangeStatus(command, content, contentWorkflow, contentRepository, schema));
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
        public void CanDeleteDraftOwn_should_not_throw_exception_if_has_permission()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Published, refToken: refToken, schema: schema);
            var command = DeleteDraftContentCommand(content);

            GuardContent.CanDeleteDraft(command, content);
        }

        [Fact]
        public void CanDeleteDraftOwn_should_throw_exception_if_has_not_permission()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Draft, refToken: refToken, schema: schema);
            var command = new DeleteContentDraft { User = user };

            Assert.Throws<DomainForbiddenException>(() => GuardContent.CanDeleteDraft(command, content));
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_singleton()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = new DeleteContent();

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        [Fact]
        public async Task CanDelete_should_throw_exception_if_referenced()
        {
            var schema = CreateSchema(true);

            var content = CreateContent(Status.Published);
            var command = new DeleteContent();

            A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, content.Id, SearchScope.All))
                .Returns(true);

            await Assert.ThrowsAsync<DomainException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        [Fact]
        public async Task CanDelete_should_not_throw_exception()
        {
            var schema = CreateSchema(false);

            var content = CreateContent(Status.Published);
            var command = new DeleteContent();

            await GuardContent.CanDelete(command, content, contentRepository, schema);
        }

        [Fact]
        public async Task CanDelete_Own_should_not_throw_exception()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");

            var content = CreateOwnContent(status: Status.Published, refToken: refToken, schema: schema);
            var command = CreateContentDeleteCommand(content);

            await GuardContent.CanDelete(command, content, contentRepository, schema);
        }

        [Fact]
        public async Task CanDeleteOwn_should_throw_exception()
        {
            var schema = CreateSchema(false);
            var refToken = new RefToken("string", "string");
            var content = CreateOwnContent(status: Status.Published, refToken: refToken, schema: schema);
            var command = new DeleteContent { User = user };

            await Assert.ThrowsAsync<DomainForbiddenException>(() => GuardContent.CanDelete(command, content, contentRepository, schema));
        }

        private void SetupCanUpdate(bool canUpdate, ClaimsPrincipal user)
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

        private IContentEntity CreateDraftContent(Status status)
        {
            return new ContentEntity
            {
                Id = DomainId.NewGuid(),
                NewStatus = status,
                AppId = appId
            };
        }

        private IContentEntity CreateContent(Status status, RefToken? refToken = null, ISchemaEntity? schema = null)
        {
            return new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Status = status,
                AppId = appId,
                CreatedBy = refToken,
                SchemaId = schema.AppId
            };
        }

        private IContentEntity CreateOwnContent(Status status, ISchemaEntity? schema, RefToken? refToken = null)
        {
            return new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Status = status,
                NewStatus = status,
                AppId = appId,
                CreatedBy = refToken,
                SchemaId = schema.AppId
            };
        }

        private DeleteContent CreateContentDeleteCommand(IContentEntity content)
        {
            return new DeleteContent
            {
                Actor = content.CreatedBy,
                User = userWithDeletePermission
            };
        }

        private ChangeContentStatus ChangeContentSatusCommand(IContentEntity content)
        {
            return new ChangeContentStatus
            {
                Status = Status.Published,
                Actor = content.CreatedBy,
                User = userWithStatusChangePermission
            };
        }

        private UpdateContent UpdateContentCommand(IContentEntity content)
        {
            return new UpdateContent
            {
                Actor = content.CreatedBy,
                User = userWithUpdatePermission,
                Data = new NamedContentData()
            };
        }

        private PatchContent PatchContentCommand(IContentEntity content)
        {
            return new PatchContent
            {
                Actor = content.CreatedBy,
                User = userWithPatchPermission,
                Data = new NamedContentData()
            };
        }

        private DeleteContentDraft DeleteDraftContentCommand(IContentEntity content)
        {
            return new DeleteContentDraft
            {
                Actor = content.CreatedBy,
                User = userWithPatchPermission
            };
        }
    }
}
