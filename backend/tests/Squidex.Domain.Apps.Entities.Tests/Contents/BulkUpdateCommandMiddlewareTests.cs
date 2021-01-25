﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class BulkUpdateCommandMiddlewareTests
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly BulkUpdateCommandMiddleware sut;

        public BulkUpdateCommandMiddlewareTests()
        {
            sut = new BulkUpdateCommandMiddleware(contentQuery, contextProvider);
        }

        [Fact]
        public async Task Should_do_nothing_if_jobs_is_null()
        {
            var command = new BulkUpdateContents();

            var result = await PublishAsync(command);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_do_nothing_if_jobs_is_empty()
        {
            var command = new BulkUpdateContents { Jobs = Array.Empty<BulkUpdateJob>() };

            var result = await PublishAsync(command);

            Assert.Empty(result);
        }

        [Fact]
        public async Task Should_throw_exception_when_content_cannot_be_resolved()
        {
            SetupContext(Permissions.AppContentsUpdate);

            var (_, _, query) = CreateTestData(true);

            var command = BulkCommand(BulkUpdateType.ChangeStatus);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == null && x.Exception is DomainObjectNotFoundException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_query_resolves_multiple_contents()
        {
            var requestContext = SetupContext(Permissions.AppContentsUpdate);

            var (id, data, query) = CreateTestData(true);

            A.CallTo(() => contentQuery.QueryAsync(requestContext, A<string>._, A<Q>.That.Matches(x => x.JsonQuery == query)))
                .Returns(ResultList.CreateFrom(2, CreateContent(id), CreateContent(id)));

            var command = BulkCommand(BulkUpdateType.ChangeStatus, query);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == null && x.Exception is DomainException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_upsert_content_with_with_resolved_id()
        {
            var requestContext = SetupContext(Permissions.AppContentsUpsert);

            var (id, data, query) = CreateTestData(true);

            A.CallTo(() => contentQuery.QueryAsync(requestContext, A<string>._, A<Q>.That.Matches(x => x.JsonQuery == query)))
                .Returns(ResultList.CreateFrom(1, CreateContent(id)));

            var command = BulkCommand(BulkUpdateType.Upsert, query: query, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_with_with_resolved_ids()
        {
            var requestContext = SetupContext(Permissions.AppContentsUpsert);

            var (_, data, query) = CreateTestData(true);

            var id1 = DomainId.NewGuid();
            var id2 = DomainId.NewGuid();

            A.CallTo(() => contentQuery.QueryAsync(requestContext, A<string>._, A<Q>.That.Matches(x => x.JsonQuery == query)))
                .Returns(ResultList.CreateFrom(2,
                    CreateContent(id1),
                    CreateContent(id2)));

            var command = BulkCommand(BulkUpdateType.Upsert, query: query, data: data);

            command.Jobs![0].ExpectedCount = 2;

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id1 && x.Exception == null);
            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id2 && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id1)))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id2)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_with_random_id_if_no_query_and_id_defined()
        {
            SetupContext(Permissions.AppContentsUpsert);

            var (_, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Upsert, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId != default && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId.ToString().Length == 36)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_with_random_id_if_query_returns_no_result()
        {
            SetupContext(Permissions.AppContentsUpsert);

            var (_, data, query) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Upsert, query: query, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId != default && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId.ToString().Length == 36)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_when_id_defined()
        {
            SetupContext(Permissions.AppContentsUpsert);

            var (id, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Upsert, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId != default && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_with_custom_id()
        {
            var requestContext = SetupContext(Permissions.AppContentsUpsert);

            var (id, data, _) = CreateTestData(true);

            var command = BulkCommand(BulkUpdateType.Upsert, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId != default && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_create_content()
        {
            SetupContext(Permissions.AppContentsCreate);

            var (id, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Create, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<CreateContent>.That.Matches(x => x.ContentId == id && x.Data == data)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_security_exception_when_user_has_no_permission_for_creating()
        {
            SetupContext(Permissions.AppContentsRead);

            var (id, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Create, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception is DomainForbiddenException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_update_content()
        {
            SetupContext(Permissions.AppContentsUpdate);

            var (id, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Update, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpdateContent>.That.Matches(x => x.ContentId == id && x.Data == data)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_security_exception_when_user_has_no_permission_for_updating()
        {
            SetupContext(Permissions.AppContentsRead);

            var (id, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Update, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception is DomainForbiddenException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_patch_content()
        {
            SetupContext(Permissions.AppContentsUpdate);

            var (id, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Patch, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<PatchContent>.That.Matches(x => x.ContentId == id && x.Data == data)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_security_exception_when_user_has_no_permission_for_patching()
        {
            SetupContext(Permissions.AppContentsRead);

            var (id, data, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Delete, id: id, data: data);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception is DomainForbiddenException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_change_content_status()
        {
            SetupContext(Permissions.AppContentsUpdate);

            var (id, _, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.ChangeStatus, id: id);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(A<ChangeContentStatus>.That.Matches(x => x.ContentId == id && x.DueTime == null)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_change_content_status_with_due_time()
        {
            SetupContext(Permissions.AppContentsUpdate);

            var time = Instant.FromDateTimeUtc(DateTime.UtcNow);

            var (id, _, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.ChangeStatus, id: id, dueTime: time);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(A<ChangeContentStatus>.That.Matches(x => x.ContentId == id && x.DueTime == time)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_security_exception_when_user_has_no_permission_for_changing_status()
        {
            SetupContext(Permissions.AppContentsRead);

            var (id, _, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.ChangeStatus, id: id);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception is DomainForbiddenException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_validate_content()
        {
            SetupContext(Permissions.AppContentsRead);

            var (id, _, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Validate, id: id);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<ValidateContent>.That.Matches(x => x.ContentId == id)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_security_exception_when_user_has_no_permission_for_validation()
        {
            SetupContext(Permissions.AppContentsDelete);

            var (id, _, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Validate, id: id);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception is DomainForbiddenException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_delete_content()
        {
            SetupContext(Permissions.AppContentsDelete);

            var (id, _, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Delete, id: id);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception == null);

            A.CallTo(() => commandBus.PublishAsync(
                    A<DeleteContent>.That.Matches(x => x.ContentId == id)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_security_exception_when_user_has_no_permission_for_deletion()
        {
            SetupContext(Permissions.AppContentsRead);

            var (id, _, _) = CreateTestData(false);

            var command = BulkCommand(BulkUpdateType.Delete, id: id);

            var result = await PublishAsync(command);

            Assert.Single(result, x => x.JobIndex == 0 && x.ContentId == id && x.Exception is DomainForbiddenException);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        private async Task<BulkUpdateResult> PublishAsync(ICommand command)
        {
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            return (context.PlainResult as BulkUpdateResult)!;
        }

        private BulkUpdateContents BulkCommand(BulkUpdateType type, Query<IJsonValue>? query = null,
            DomainId? id = null, ContentData? data = null, Instant? dueTime = null)
        {
            return new BulkUpdateContents
            {
                AppId = appId,
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = type,
                        Id = id,
                        Data = data!,
                        DueTime = dueTime,
                        Query = query,
                    }
                },
                SchemaId = schemaId
            };
        }

        private Context SetupContext(string id)
        {
            var permission = Permissions.ForApp(id, appId.Name, schemaId.Name).Id;

            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            claimsIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, permission));

            var requestContext = new Context(claimsPrincipal);

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            return requestContext;
        }

        private static (DomainId Id, ContentData Data, Query<IJsonValue>? Query) CreateTestData(bool withQuery)
        {
            Query<IJsonValue>? query = withQuery ? new Query<IJsonValue>() : null;

            var data =
                new ContentData()
                    .AddField("value",
                        new ContentFieldData()
                            .AddJsonValue("iv", JsonValue.Create(1)));

            return (DomainId.NewGuid(), data, query);
        }

        private static IEnrichedContentEntity CreateContent(DomainId id)
        {
            return new ContentEntity { Id = id };
        }
    }
}
