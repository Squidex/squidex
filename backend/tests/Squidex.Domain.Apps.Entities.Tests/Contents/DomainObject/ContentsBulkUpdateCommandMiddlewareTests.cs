﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public class ContentsBulkUpdateCommandMiddlewareTests : GivenContext
{
    private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
    private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly NamedId<DomainId> schemaCustomId = NamedId.Of(DomainId.NewGuid(), "my-schema2");
    private readonly Instant time = Instant.FromDateTimeUtc(DateTime.UtcNow);
    private readonly ContentsBulkUpdateCommandMiddleware sut;

    public ContentsBulkUpdateCommandMiddlewareTests()
    {
        sut = new ContentsBulkUpdateCommandMiddleware(contentQuery, contextProvider);
    }

    [Fact]
    public async Task Should_do_nothing_if_jobs_is_null()
    {
        var command = new BulkUpdateContents();

        var actual = await PublishAsync(command);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_do_nothing_if_jobs_is_empty()
    {
        var command = new BulkUpdateContents { Jobs = [] };

        var actual = await PublishAsync(command);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_throw_exception_if_content_cannot_be_resolved()
    {
        SetupContext(PermissionIds.AppContentsUpdateOwn);

        CreateTestData(true);

        var command = BulkCommand(BulkUpdateContentType.ChangeStatus);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == null && x.Exception is DomainObjectNotFoundException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_query_resolves_multiple_contents()
    {
        var requestContext = SetupContext(PermissionIds.AppContentsUpdateOwn);

        var (_, _, query) = CreateTestData(true);

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x =>
                    x.NoCleanup() &&
                    x.NoEnrichment() &&
                    x.NoTotal()),
                schemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.JsonQuery == query), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(2, CreateContent(), CreateContent()));

        var command = BulkCommand(BulkUpdateContentType.ChangeStatus, new BulkUpdateJob { Query = query });

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == null && x.Exception is DomainException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_upsert_content_with_resolved_id()
    {
        var requestContext = SetupContext(PermissionIds.AppContentsUpsert);

        var (id, data, query) = CreateTestData(true);

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x =>
                    x.NoCleanup() &&
                    x.NoEnrichment() &&
                    x.NoTotal()),
                schemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.JsonQuery == query),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, CreateContent().WithId(id)));

        var command = BulkCommand(BulkUpdateContentType.Upsert, new BulkUpdateJob { Query = query, Data = data });

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_upsert_content_with_resolved_ids()
    {
        var requestContext = SetupContext(PermissionIds.AppContentsUpsert);

        var (id, data, query) = CreateTestData(true);

        var content1 = CreateContent();
        var content2 = CreateContent();

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x =>
                    x.NoCleanup() &&
                    x.NoEnrichment() &&
                    x.NoTotal()),
                schemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.JsonQuery == query),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(2, content1, content2));

        var command = BulkCommand(BulkUpdateContentType.Upsert, new BulkUpdateJob { Query = query, Data = data });

        command.Jobs![0].ExpectedCount = 2;

        var actual = await PublishAsync(command);

        Assert.Equal(2, actual.Count);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == content1.Id && x.Exception == null);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == content2.Id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == content1.Id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == content2.Id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_upsert_content_with_random_id_if_no_query_and_id_defined()
    {
        SetupContext(PermissionIds.AppContentsUpsert);

        var (_, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Upsert, new BulkUpdateJob { Data = data });

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id != default && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId != default), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_upsert_content_with_random_id_if_query_returns_no_result()
    {
        SetupContext(PermissionIds.AppContentsUpsert);

        var (_, data, query) = CreateTestData(true);

        var command = BulkCommand(BulkUpdateContentType.Upsert, new BulkUpdateJob { Query = query, Data = data });

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id != default && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId != default), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_upsert_content_if_id_defined()
    {
        SetupContext(PermissionIds.AppContentsUpsert);

        var (id, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Upsert, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id != default && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_upsert_content_with_custom_id()
    {
        SetupContext(PermissionIds.AppContentsUpsert);

        var (id, data, _) = CreateTestData(true);

        var command = BulkCommand(BulkUpdateContentType.Upsert, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id != default && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_create_content()
    {
        SetupContext(PermissionIds.AppContentsCreate);

        var (id, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Create, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x => x.ContentId == id && x.Data == data), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_creating()
    {
        SetupContext(PermissionIds.AppContentsReadOwn);

        var (id, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Create, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_update_content()
    {
        SetupContext(PermissionIds.AppContentsUpdateOwn);

        var (id, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Update, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpdateContent>.That.Matches(x => x.ContentId == id && x.Data == data), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_updating()
    {
        SetupContext(PermissionIds.AppContentsReadOwn);

        var (id, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Update, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_update_content_defaults()
    {
        SetupContext(PermissionIds.AppContentsUpdateOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.EnrichDefaults, new BulkUpdateJob(), id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<EnrichContentDefaults>.That.Matches(x => x.ContentId == id), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_defaults()
    {
        SetupContext(PermissionIds.AppContentsReadOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.EnrichDefaults, new BulkUpdateJob(), id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_patch_content()
    {
        SetupContext(PermissionIds.AppContentsUpdateOwn);

        var (id, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Patch, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<PatchContent>.That.Matches(x => x.ContentId == id && x.Data == data), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_patching()
    {
        SetupContext(PermissionIds.AppContentsReadOwn);

        var (id, data, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Delete, new BulkUpdateJob { Data = data }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_change_content_status()
    {
        SetupContext(PermissionIds.AppContentsChangeStatusOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.ChangeStatus, id: id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x => x.ContentId == id && x.DueTime == null), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_change_content_status_with_due_time()
    {
        SetupContext(PermissionIds.AppContentsChangeStatusOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.ChangeStatus, new BulkUpdateJob { DueTime = time }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x => x.ContentId == id && x.DueTime == time), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_changing_status()
    {
        SetupContext(PermissionIds.AppContentsReadOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.ChangeStatus, id: id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_validate_content()
    {
        SetupContext(PermissionIds.AppContentsReadOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Validate, id: id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<ValidateContent>.That.Matches(x => x.ContentId == id), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_validation()
    {
        SetupContext(PermissionIds.AppContentsDeleteOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Validate, id: id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_delete_content()
    {
        SetupContext(PermissionIds.AppContentsDeleteOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Delete, id: id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<DeleteContent>.That.Matches(x => x.ContentId == id), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_security_exception_if_user_has_no_permission_for_deletion()
    {
        SetupContext(PermissionIds.AppContentsReadOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Delete, id: id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainForbiddenException);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_override_schema_name()
    {
        SetupContext(PermissionIds.AppContentsDeleteOwn);

        A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(A<Context>._, schemaCustomId.Name, A<CancellationToken>._))
            .Returns(Schema.WithId(schemaCustomId));

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Delete, new BulkUpdateJob { Schema = schemaCustomId.Name }, id);

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception == null);

        A.CallTo(() => commandBus.PublishAsync(
                A<DeleteContent>.That.Matches(x => x.SchemaId == schemaCustomId), A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_schema_name_not_defined()
    {
        SetupContext(PermissionIds.AppContentsDeleteOwn);

        var (id, _, _) = CreateTestData(false);

        var command = BulkCommand(BulkUpdateContentType.Delete, new BulkUpdateJob(), id);

        // Unset schema id, so that no schema id is set for the command.
        command.SchemaId = null!;

        var actual = await PublishAsync(command);

        Assert.Single(actual);
        Assert.Single(actual, x => x.JobIndex == 0 && x.Id == id && x.Exception is DomainObjectNotFoundException);

        A.CallTo(() => commandBus.PublishAsync(
                A<DeleteContent>.That.Matches(x => x.SchemaId == schemaCustomId), A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private async Task<BulkUpdateResult> PublishAsync(ICommand command)
    {
        var context = new CommandContext(command, commandBus);

        await sut.HandleAsync(context, CancellationToken);

        return (context.PlainResult as BulkUpdateResult)!;
    }

    private BulkUpdateContents BulkCommand(BulkUpdateContentType type, BulkUpdateJob? job = null, DomainId? id = null)
    {
        job ??= new BulkUpdateJob();
        job.Id = id;
        job.Type = type;

        return new BulkUpdateContents
        {
            AppId = AppId,
            Jobs =
            [
                job,
            ],
            SchemaId = schemaId,
        };
    }

    private Context SetupContext(string id)
    {
        var requestContext = CreateContext(false,
            PermissionIds.ForApp(id, AppId.Name, schemaId.Name).Id,
            PermissionIds.ForApp(id, AppId.Name, schemaCustomId.Name).Id);

        A.CallTo(() => contextProvider.Context)
            .Returns(requestContext);

        return requestContext;
    }

    private static (DomainId Id, ContentData Data, Query<JsonValue>? Query) CreateTestData(bool withQuery)
    {
        Query<JsonValue>? query = withQuery ? new Query<JsonValue>() : null;

        var data =
            new ContentData()
                .AddField("value",
                    new ContentFieldData()
                        .AddInvariant(1));

        return (DomainId.NewGuid(), data, query);
    }
}
