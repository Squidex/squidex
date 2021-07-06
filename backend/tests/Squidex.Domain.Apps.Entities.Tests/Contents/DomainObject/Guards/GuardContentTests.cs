// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
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
        private readonly ISchemaEntity normalSchema;
        private readonly ISchemaEntity normalUnpublishedSchema;
        private readonly ISchemaEntity singletonSchema;
        private readonly ISchemaEntity singletonUnpublishedSchema;
        private readonly ClaimsPrincipal user = Mocks.FrontendUser();
        private readonly RefToken actor = RefToken.User("123");

        public GuardContentTests()
        {
            normalUnpublishedSchema =
                Mocks.Schema(appId, schemaId, new Schema(schemaId.Name));

            normalSchema =
                Mocks.Schema(appId, schemaId, new Schema(schemaId.Name).Publish());

            singletonUnpublishedSchema =
                Mocks.Schema(appId, schemaId, new Schema(schemaId.Name, type: SchemaType.Singleton));

            singletonSchema =
                Mocks.Schema(appId, schemaId, new Schema(schemaId.Name, type: SchemaType.Singleton).Publish());
        }

        [Fact]
        public void Should_throw_exception_if_creating_content_for_unpublished_schema()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalUnpublishedSchema);

            Assert.Throws<DomainException>(() => context.MustNotCreateForUnpublishedSchema());
        }

        [Fact]
        public void Should_not_throw_exception_if_creating_content_for_unpublished_singleton()
        {
            var context = CreateContext(CreateContent(Status.Draft, singletonSchema.Id), singletonUnpublishedSchema);

            context.MustNotCreateSingleton();
        }

        [Fact]
        public void Should_not_throw_exception_if_creating_content_for_published_schema()
        {
            var context = CreateContext(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

            context.MustNotCreateSingleton();
        }

        [Fact]
        public void Should_not_throw_exception_if_creating_content_for_published_singleton_schema()
        {
            var context = CreateContext(CreateContent(Status.Draft, singletonSchema.Id), singletonSchema);

            context.MustNotCreateSingleton();
        }

        [Fact]
        public void Should_throw_exception_if_creating_singleton_content()
        {
            var context = CreateContext(CreateContent(Status.Draft), singletonSchema);

            Assert.Throws<DomainException>(() => context.MustNotCreateSingleton());
        }

        [Fact]
        public void Should_not_throw_exception_if_creating_singleton_content_with_schema_id()
        {
            var context = CreateContext(CreateContent(Status.Draft, singletonSchema.Id), singletonSchema);

            context.MustNotCreateSingleton();
        }

        [Fact]
        public void Should_not_throw_exception_if_creating_non_singleton_content()
        {
            var context = CreateContext(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

            context.MustNotCreateSingleton();
        }

        [Fact]
        public void Should_throw_exception_if_changing_singleton_content()
        {
            var context = CreateContext(CreateContent(Status.Draft), singletonSchema);

            Assert.Throws<DomainException>(() => context.MustNotChangeSingleton(Status.Archived));
        }

        [Fact]
        public void Should_not_throw_exception_if_changing_singleton_to_published()
        {
            var context = CreateContext(CreateDraftContent(Status.Published, singletonSchema.Id), singletonSchema);

            context.MustNotChangeSingleton(Status.Published);
        }

        [Fact]
        public void Should_not_throw_exception_if_changing_non_singleton_content()
        {
            var context = CreateContext(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

            context.MustNotChangeSingleton(Status.Archived);
        }

        [Fact]
        public void Should_throw_exception_if_deleting_singleton_content()
        {
            var context = CreateContext(CreateContent(Status.Draft), singletonSchema);

            Assert.Throws<DomainException>(() => context.MustNotDeleteSingleton());
        }

        [Fact]
        public void Should_not_throw_exception_if_deleting_non_singleton_content()
        {
            var context = CreateContext(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

            context.MustNotDeleteSingleton();
        }

        [Fact]
        public void Should_throw_exception_if_draft_already_created()
        {
            var context = CreateContext(CreateDraftContent(Status.Draft), normalSchema);

            Assert.Throws<DomainException>(() => context.MustCreateDraft());
        }

        [Fact]
        public void Should_throw_exception_if_draft_cannot_be_created()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            Assert.Throws<DomainException>(() => context.MustCreateDraft());
        }

        [Fact]
        public void Should_not_throw_exception_if_draft_can_be_created()
        {
            var context = CreateContext(CreateContent(Status.Published), normalSchema);

            context.MustCreateDraft();
        }

        [Fact]
        public void Should_throw_exception_if_draft_cannot_be_deleted()
        {
            var context = CreateContext(CreateContent(Status.Published), normalSchema);

            Assert.Throws<DomainException>(() => context.MustDeleteDraft());
        }

        [Fact]
        public void Should_not_throw_exception_if_draft_can_be_deleted()
        {
            var context = CreateContext(CreateDraftContent(Status.Draft), normalSchema);

            context.MustDeleteDraft();
        }

        [Fact]
        public void Should_throw_exception_if_data_is_not_defined()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            Assert.Throws<ValidationException>(() => context.MustHaveData(null));
        }

        [Fact]
        public void Should_not_throw_exception_if_data_is_defined()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            context.MustHaveData(new ContentData());
        }

        [Fact]
        public async Task Should_provide_initial_status()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            A.CallTo(() => contentWorkflow.GetInitialStatusAsync(context.Schema))
                .Returns(Status.Archived);

            Assert.Equal(Status.Archived, await context.GetInitialStatusAsync());
        }

        [Fact]
        public async Task Should_throw_exception_if_workflow_permits_update()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            A.CallTo(() => contentWorkflow.CanUpdateAsync(context.Content, context.Content.EditingStatus(), context.User))
                .Returns(false);

            await Assert.ThrowsAsync<DomainException>(() => context.CheckUpdateAsync());
        }

        [Fact]
        public async Task Should_not_throw_exception_if_workflow_allows_update()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            A.CallTo(() => contentWorkflow.CanUpdateAsync(context.Content, context.Content.EditingStatus(), context.User))
                .Returns(true);

            await context.CheckUpdateAsync();
        }

        [Fact]
        public async Task Should_throw_exception_if_workflow_status_not_valid()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            A.CallTo(() => contentWorkflow.GetInfoAsync((ContentEntity)context.Content, Status.Archived))
                .Returns(Task.FromResult<StatusInfo?>(null));

            await Assert.ThrowsAsync<ValidationException>(() => context.CheckStatusAsync(Status.Archived));
        }

        [Fact]
        public async Task Should_not_throw_exception_if_workflow_status_is_valid()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            A.CallTo(() => contentWorkflow.GetInfoAsync((ContentEntity)context.Content, Status.Archived))
                .Returns(new StatusInfo(Status.Archived, StatusColors.Archived));

            await context.CheckStatusAsync(Status.Archived);
        }

        [Fact]
        public async Task Should_not_throw_exception_if_workflow_status_is_checked_for_singleton()
        {
            var context = CreateContext(CreateContent(Status.Draft), singletonSchema);

            await context.CheckStatusAsync(Status.Archived);

            A.CallTo(() => contentWorkflow.GetInfoAsync((ContentEntity)context.Content, Status.Archived))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_if_workflow_transition_not_valid()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            A.CallTo(() => contentWorkflow.CanMoveToAsync((ContentEntity)context.Content, Status.Draft, Status.Archived, context.User))
                .Returns(false);

            await Assert.ThrowsAsync<ValidationException>(() => context.CheckTransitionAsync(Status.Archived));
        }

        [Fact]
        public async Task Should_not_throw_exception_if_workflow_transition_is_valid()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            A.CallTo(() => contentWorkflow.CanMoveToAsync((ContentEntity)context.Content, Status.Draft, Status.Archived, context.User))
                .Returns(true);

            await context.CheckTransitionAsync(Status.Archived);
        }

        [Fact]
        public async Task Should_not_throw_exception_if_workflow_transition_is_checked_for_singleton()
        {
            var context = CreateContext(CreateContent(Status.Draft), singletonSchema);

            await context.CheckTransitionAsync(Status.Archived);

            A.CallTo(() => contentWorkflow.CanMoveToAsync((ContentEntity)context.Content, A<Status>._, A<Status>._, A<ClaimsPrincipal>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_throw_exception_if_content_is_from_another_user_but_user_has_permission()
        {
            var userPermission = Permissions.ForApp(Permissions.AppContentsDelete, appId.Name, schemaId.Name).Id;
            var userObject = Mocks.FrontendUser(permission: userPermission);

            var context = CreateContext(CreateContent(Status.Draft), normalSchema, userObject);

            ((ContentEntity)context.Content).CreatedBy = RefToken.User("456");

            context.MustHavePermission(Permissions.AppContentsDelete);
        }

        [Fact]
        public void Should_not_throw_exception_if_content_is_from_current_user()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            ((ContentEntity)context.Content).CreatedBy = actor;

            context.MustHavePermission(Permissions.AppContentsDelete);
        }

        [Fact]
        public void Should_not_throw_exception_if_user_is_null()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema, null);

            ((ContentEntity)context.Content).CreatedBy = RefToken.User("456");

            context.MustHavePermission(Permissions.AppContentsDelete);
        }

        [Fact]
        public void Should_throw_exception_if_content_is_from_another_user_and_user_has_no_permission()
        {
            var context = CreateContext(CreateContent(Status.Draft), normalSchema);

            ((ContentEntity)(ContentEntity)context.Content).CreatedBy = RefToken.User("456");

            Assert.Throws<DomainForbiddenException>(() => context.MustHavePermission(Permissions.AppContentsDelete));
        }

        private OperationContext CreateContext(ContentEntity content, ISchemaEntity contextSchema)
        {
            return CreateContext(content, contextSchema, user);
        }

        private OperationContext CreateContext(ContentEntity content, ISchemaEntity contextSchema, ClaimsPrincipal? currentUser)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddSingleton(contentRepository)
                    .AddSingleton(contentWorkflow)
                    .BuildServiceProvider();

            return new OperationContext(serviceProvider)
            {
                Actor = actor,
                App = Mocks.App(appId),
                ContentProvider = () => content,
                ContentId = content.Id,
                Schema = contextSchema,
                User = currentUser
            };
        }

        private ContentEntity CreateDraftContent(Status status, DomainId? id = null)
        {
            return CreateContentCore(new ContentEntity { NewStatus = status }, id);
        }

        private ContentEntity CreateContent(Status status, DomainId? id = null)
        {
            return CreateContentCore(new ContentEntity { Status = status }, id);
        }

        private ContentEntity CreateContentCore(ContentEntity content, DomainId? id = null)
        {
            content.Id = id ?? DomainId.NewGuid();
            content.AppId = appId;
            content.Created = default;
            content.CreatedBy = actor;
            content.SchemaId = schemaId;

            return content;
        }
    }
}
