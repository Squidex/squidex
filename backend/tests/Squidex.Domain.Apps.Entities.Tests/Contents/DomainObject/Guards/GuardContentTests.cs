// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
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

#pragma warning disable CA2012 // Use ValueTasks correctly

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards;

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
    private readonly ISchemaEntity componentSchema;
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

        componentSchema =
            Mocks.Schema(appId, schemaId, new Schema(schemaId.Name, type: SchemaType.Component).Publish());
    }

    [Fact]
    public void Should_throw_exception_if_creating_content_for_unpublished_schema()
    {
        var operation = Operation(CreateContent(Status.Draft), normalUnpublishedSchema);

        Assert.Throws<DomainException>(() => operation.MustNotCreateForUnpublishedSchema());
    }

    [Fact]
    public void Should_not_throw_exception_if_creating_content_for_unpublished_singleton()
    {
        var operation = Operation(CreateContent(Status.Draft, singletonSchema.Id), singletonUnpublishedSchema);

        operation.MustNotCreateSingleton();
    }

    [Fact]
    public void Should_not_throw_exception_if_creating_content_for_published_schema()
    {
        var operation = Operation(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

        operation.MustNotCreateSingleton();
    }

    [Fact]
    public void Should_not_throw_exception_if_creating_content_for_published_singleton_schema()
    {
        var operation = Operation(CreateContent(Status.Draft, singletonSchema.Id), singletonSchema);

        operation.MustNotCreateSingleton();
    }

    [Fact]
    public void Should_throw_exception_if_creating_singleton_content()
    {
        var operation = Operation(CreateContent(Status.Draft), singletonSchema);

        Assert.Throws<DomainException>(() => operation.MustNotCreateSingleton());
    }

    [Fact]
    public void Should_throw_exception_if_creating_component_content()
    {
        var operation = Operation(CreateContent(Status.Draft), componentSchema);

        Assert.Throws<DomainException>(() => operation.MustNotCreateComponent());
    }

    [Fact]
    public void Should_not_throw_exception_if_creating_singleton_content_with_schema_id()
    {
        var operation = Operation(CreateContent(Status.Draft, singletonSchema.Id), singletonSchema);

        operation.MustNotCreateSingleton();
    }

    [Fact]
    public void Should_not_throw_exception_if_creating_non_singleton_content()
    {
        var operation = Operation(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

        operation.MustNotCreateSingleton();
    }

    [Fact]
    public void Should_throw_exception_if_changing_singleton_content()
    {
        var operation = Operation(CreateContent(Status.Draft), singletonSchema);

        Assert.Throws<DomainException>(() => operation.MustNotChangeSingleton(Status.Archived));
    }

    [Fact]
    public void Should_not_throw_exception_if_changing_singleton_to_published()
    {
        var operation = Operation(CreateDraftContent(Status.Published, singletonSchema.Id), singletonSchema);

        operation.MustNotChangeSingleton(Status.Published);
    }

    [Fact]
    public void Should_not_throw_exception_if_changing_non_singleton_content()
    {
        var operation = Operation(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

        operation.MustNotChangeSingleton(Status.Archived);
    }

    [Fact]
    public void Should_throw_exception_if_deleting_singleton_content()
    {
        var operation = Operation(CreateContent(Status.Draft), singletonSchema);

        Assert.Throws<DomainException>(() => operation.MustNotDeleteSingleton());
    }

    [Fact]
    public void Should_not_throw_exception_if_deleting_non_singleton_content()
    {
        var operation = Operation(CreateContent(Status.Draft, singletonSchema.Id), normalSchema);

        operation.MustNotDeleteSingleton();
    }

    [Fact]
    public void Should_throw_exception_if_draft_already_created()
    {
        var operation = Operation(CreateDraftContent(Status.Draft), normalSchema);

        Assert.Throws<DomainException>(() => operation.MustCreateDraft());
    }

    [Fact]
    public void Should_throw_exception_if_draft_cannot_be_created()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        Assert.Throws<DomainException>(() => operation.MustCreateDraft());
    }

    [Fact]
    public void Should_not_throw_exception_if_draft_can_be_created()
    {
        var operation = Operation(CreateContent(Status.Published), normalSchema);

        operation.MustCreateDraft();
    }

    [Fact]
    public void Should_throw_exception_if_draft_cannot_be_deleted()
    {
        var operation = Operation(CreateContent(Status.Published), normalSchema);

        Assert.Throws<DomainException>(() => operation.MustDeleteDraft());
    }

    [Fact]
    public void Should_not_throw_exception_if_draft_can_be_deleted()
    {
        var operation = Operation(CreateDraftContent(Status.Draft), normalSchema);

        operation.MustDeleteDraft();
    }

    [Fact]
    public void Should_throw_exception_if_data_is_not_defined()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        Assert.Throws<ValidationException>(() => operation.MustHaveData(null));
    }

    [Fact]
    public void Should_not_throw_exception_if_data_is_defined()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        operation.MustHaveData(new ContentData());
    }

    [Fact]
    public async Task Should_provide_initial_status()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentWorkflow.GetInitialStatusAsync(operation.Schema))
            .Returns(Status.Archived);

        Assert.Equal(Status.Archived, await operation.GetInitialStatusAsync());
    }

    [Fact]
    public async Task Should_throw_exception_if_workflow_permits_update()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentWorkflow.CanUpdateAsync(operation.Snapshot, operation.Snapshot.EditingStatus(), operation.User))
            .Returns(false);

        await Assert.ThrowsAsync<DomainException>(() => operation.CheckUpdateAsync());
    }

    [Fact]
    public async Task Should_not_throw_exception_if_workflow_allows_update()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentWorkflow.CanUpdateAsync(operation.Snapshot, operation.Snapshot.EditingStatus(), operation.User))
            .Returns(true);

        await operation.CheckUpdateAsync();
    }

    [Fact]
    public async Task Should_throw_exception_if_workflow_status_not_valid()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentWorkflow.GetInfoAsync((ContentEntity)operation.Snapshot, Status.Archived))
            .Returns(ValueTask.FromResult<StatusInfo?>(null));

        await Assert.ThrowsAsync<ValidationException>(() => operation.CheckStatusAsync(Status.Archived));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_workflow_status_is_valid()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentWorkflow.GetInfoAsync((ContentEntity)operation.Snapshot, Status.Archived))
            .Returns(new StatusInfo(Status.Archived, StatusColors.Archived));

        await operation.CheckStatusAsync(Status.Archived);
    }

    [Fact]
    public async Task Should_not_throw_exception_if_workflow_status_is_checked_for_singleton()
    {
        var operation = Operation(CreateContent(Status.Draft), singletonSchema);

        await operation.CheckStatusAsync(Status.Archived);

        A.CallTo(() => contentWorkflow.GetInfoAsync((ContentEntity)operation.Snapshot, Status.Archived))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_workflow_transition_not_valid()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentWorkflow.CanMoveToAsync((ContentEntity)operation.Snapshot, Status.Draft, Status.Archived, operation.User))
            .Returns(false);

        await Assert.ThrowsAsync<ValidationException>(() => operation.CheckTransitionAsync(Status.Archived));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_workflow_transition_is_valid()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentWorkflow.CanMoveToAsync((ContentEntity)operation.Snapshot, Status.Draft, Status.Archived, operation.User))
            .Returns(true);

        await operation.CheckTransitionAsync(Status.Archived);
    }

    [Fact]
    public async Task Should_not_throw_exception_if_workflow_transition_is_checked_for_singleton()
    {
        var operation = Operation(CreateContent(Status.Draft), singletonSchema);

        await operation.CheckTransitionAsync(Status.Archived);

        A.CallTo(() => contentWorkflow.CanMoveToAsync((ContentEntity)operation.Snapshot, A<Status>._, A<Status>._, A<ClaimsPrincipal>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public void Should_not_throw_exception_if_content_is_from_another_user_but_user_has_permission()
    {
        var userPermission = PermissionIds.ForApp(PermissionIds.AppContentsDelete, appId.Name, schemaId.Name).Id;
        var userObject = Mocks.FrontendUser(permission: userPermission);

        var operation = Operation(CreateContent(Status.Draft), normalSchema, userObject);

        ((ContentEntity)operation.Snapshot).CreatedBy = RefToken.User("456");

        operation.MustHavePermission(PermissionIds.AppContentsDelete);
    }

    [Fact]
    public void Should_not_throw_exception_if_content_is_from_current_user()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        ((ContentEntity)operation.Snapshot).CreatedBy = actor;

        operation.MustHavePermission(PermissionIds.AppContentsDelete);
    }

    [Fact]
    public void Should_not_throw_exception_if_user_is_null()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema, null);

        ((ContentEntity)operation.Snapshot).CreatedBy = RefToken.User("456");

        operation.MustHavePermission(PermissionIds.AppContentsDelete);
    }

    [Fact]
    public void Should_throw_exception_if_content_is_from_another_user_and_user_has_no_permission()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        ((ContentEntity)operation.Snapshot).CreatedBy = RefToken.User("456");

        Assert.Throws<DomainForbiddenException>(() => operation.MustHavePermission(PermissionIds.AppContentsDelete));
    }

    [Fact]
    public async Task Should_throw_exception_if_referenced()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, operation.CommandId, SearchScope.All, default))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => operation.CheckReferrersAsync());
    }

    [Fact]
    public async Task Should_not_throw_exception_if_not_referenced()
    {
        var operation = Operation(CreateContent(Status.Draft), normalSchema);

        A.CallTo(() => contentRepository.HasReferrersAsync(appId.Id, operation.CommandId, SearchScope.All, default))
            .Returns(true);

        await Assert.ThrowsAsync<DomainException>(() => operation.CheckReferrersAsync());
    }

    private ContentOperation Operation(ContentEntity content, ISchemaEntity operationSchema)
    {
        return Operation(content, operationSchema, Mocks.FrontendUser());
    }

    private ContentOperation Operation(ContentEntity content, ISchemaEntity operationSchema, ClaimsPrincipal? currentUser)
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(contentRepository)
                .AddSingleton(contentWorkflow)
                .BuildServiceProvider();

        return new ContentOperation(serviceProvider, () => content)
        {
            App = Mocks.App(appId),
            Command = new CreateContent { User = currentUser, Actor = actor },
            CommandId = content.Id,
            Schema = operationSchema
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
