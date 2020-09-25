// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class BulkUpdateCommandMiddlewareTests
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly ICommandBus commandBus = A.Dummy<ICommandBus>();
        private readonly Context requestContext = Context.Anonymous();
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly BulkUpdateCommandMiddleware sut;

        public BulkUpdateCommandMiddlewareTests()
        {
            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new BulkUpdateCommandMiddleware(contentQuery, contextProvider);
        }

        [Fact]
        public async Task Should_do_nothing_if_jobs_is_null()
        {
            var command = new BulkUpdateContents();

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.True(context.PlainResult is BulkUpdateResult);
        }

        [Fact]
        public async Task Should_do_nothing_if_jobs_is_empty()
        {
            var command = new BulkUpdateContents { Jobs = Array.Empty<BulkUpdateJob>() };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.True(context.PlainResult is BulkUpdateResult);
        }

        [Fact]
        public async Task Should_upsert_content_with_random_id_if_no_query_and_id_defined()
        {
            var (_, data, _) = CreateTestData(false);

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.Upsert,
                        Data = data
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId != default && x.Exception == null));

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId.ToString().Length == 36)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_with_random_id_if_query_returns_no_result()
        {
            var (_, data, query) = CreateTestData(false);

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.Upsert,
                        Data = data,
                        Query = query
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId != default && x.Exception == null));

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId.ToString().Length == 36)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_when_id_defined()
        {
            var (id, data, _) = CreateTestData(false);

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.Upsert,
                        Data = data,
                        Id = id
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId != default && x.Exception == null));

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_upsert_content_with_custom_id()
        {
            var (id, data, query) = CreateTestData(true);

            A.CallTo(() => contentQuery.QueryAsync(requestContext, A<string>._, A<Q>.That.Matches(x => x.ParsedJsonQuery == query)))
                .Returns(ResultList.CreateFrom(1, CreateContent(id)));

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.Upsert,
                        Data = data,
                        Query = query
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId != default && x.Exception == null));

            A.CallTo(() => commandBus.PublishAsync(
                    A<UpsertContent>.That.Matches(x => x.Data == data && x.ContentId == id)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_throw_exception_when_query_resolves_multiple_contents()
        {
            var (id, data, query) = CreateTestData(true);

            A.CallTo(() => contentQuery.QueryAsync(requestContext, A<string>._, A<Q>.That.Matches(x => x.ParsedJsonQuery == query)))
                .Returns(ResultList.CreateFrom(2, CreateContent(id), CreateContent(id)));

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.Upsert,
                        Data = data,
                        Query = query
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId == null && x.Exception is DomainException));

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_change_content_status()
        {
            var (id, _, _) = CreateTestData(false);

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.ChangeStatus,
                        Id = id
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId == id));

            A.CallTo(() => commandBus.PublishAsync(A<ChangeContentStatus>.That.Matches(x => x.ContentId == id)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_content_id_to_change_cannot_be_resolved()
        {
            var (_, _, query) = CreateTestData(true);

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.ChangeStatus,
                        Query = query
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId == null && x.Exception is DomainObjectNotFoundException));

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_delete_content()
        {
            var (id, _, _) = CreateTestData(false);

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.Delete,
                        Id = id
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId == id));

            A.CallTo(() => commandBus.PublishAsync(
                    A<DeleteContent>.That.Matches(x => x.ContentId == id)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_when_content_id_to_delete_cannot_be_resolved()
        {
            var (_, _, query) = CreateTestData(true);

            var command = new BulkUpdateContents
            {
                Jobs = new[]
                {
                    new BulkUpdateJob
                    {
                        Type = BulkUpdateType.Delete,
                        Query = query
                    }
                },
                SchemaId = schemaId
            };

            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            var result = context.Result<BulkUpdateResult>();

            Assert.Single(result);
            Assert.Equal(1, result.Count(x => x.ContentId == null && x.Exception is DomainObjectNotFoundException));

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        private static (DomainId Id, NamedContentData Data, Query<IJsonValue>? Query) CreateTestData(bool withQuery)
        {
            Query<IJsonValue>? query = withQuery ? new Query<IJsonValue>() : null;

            var data =
                new NamedContentData()
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
