﻿// ==========================================================================
//  Squidex Headless CMS
// ================================ ==========================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FakeItEasy;
using GraphQL;
using GraphQL.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLMutationTests : GraphQLTestBase
    {
        private readonly Guid contentId = Guid.NewGuid();
        private readonly IEnrichedContentEntity content;
        private readonly CommandContext commandContext = new CommandContext(new PatchContent(), A.Dummy<ICommandBus>());

        public GraphQLMutationTests()
        {
            content = TestContent.Create(schemaId, contentId, schemaRefId1.Id, schemaRefId2.Id, null);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored))
                .Returns(commandContext);
        }

        [Fact]
        public async Task Should_return_single_content_when_creating_content()
        {
            var query = @"
                mutation {
                  createMySchemaContent(data: <DATA>) {
                    <FIELDS>
                  }
                }".Replace("<DATA>", GetDataString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    createMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x =>
                    x.SchemaId.Equals(schemaId) &&
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_when_creating_content_with_variable()
        {
            var query = @"
                mutation OP($data: MySchemaDataInputDto!) {
                  createMySchemaContent(data: $data) {
                    <FIELDS>
                  }
                }".Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query, Inputs = GetInput() });

            var expected = new
            {
                data = new
                {
                    createMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x =>
                    x.SchemaId.Equals(schemaId) &&
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_when_updating_content()
        {
            var query = @"
                mutation {
                  updateMySchemaContent(id: ""<ID>"", data: <DATA>, expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<DATA>", GetDataString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    updateMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<UpdateContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_when_updating_content_with_variable()
        {
            var query = @"
                mutation OP($data: MySchemaDataInputDto!) {
                  updateMySchemaContent(id: ""<ID>"", data: $data, expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query, Inputs = GetInput() });

            var expected = new
            {
                data = new
                {
                    updateMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<UpdateContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_when_patching_content()
        {
            var query = @"
                mutation {
                  patchMySchemaContent(id: ""<ID>"", data: <DATA>, expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<DATA>", GetDataString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    patchMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<PatchContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_when_patching_content_with_variable()
        {
            var query = @"
                mutation OP($data: MySchemaDataInputDto!) {
                  patchMySchemaContent(id: ""<ID>"", data: $data, expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query, Inputs = GetInput() });

            var expected = new
            {
                data = new
                {
                    patchMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<PatchContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_status_change()
        {
            var dueTime = SystemClock.Instance.GetCurrentInstant().WithoutMs();

            var query = @"
                mutation {
                  publishMySchemaContent(id: ""<ID>"", status: ""Published"", dueTime: ""<TIME>"", expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<TIME>", dueTime.ToString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    publishMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.DueTime == dueTime &&
                    x.ExpectedVersion == 10 &&
                    x.Status == Status.Published)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_delete()
        {
            var query = @"
                mutation {
                  deleteMySchemaContent(id: ""<ID>"", expectedVersion: 10) {
                    version 
                  }
                }".Replace("<ID>", contentId.ToString());

            commandContext.Complete(new EntitySavedResult(13));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    deleteMySchemaContent = new
                    {
                        version = 13
                    }
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<DeleteContent>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.ExpectedVersion == 10)))
                .MustHaveHappened();
        }

        private Inputs GetInput()
        {
            var input = new
            {
                data = TestContent.Data(content, schemaRefId1.Id, schemaRefId2.Id)
            };

            return JObject.FromObject(input).ToInputs();
        }

        private string GetDataString()
        {
            var data = TestContent.Data(content, schemaRefId1.Id, schemaRefId2.Id);

            var json = JsonConvert.SerializeObject(data);

            return Regex.Replace(json, "\"([^\"]+)\":", x => x.Groups[1].Value + ":");
        }
    }
}
