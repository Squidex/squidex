// ==========================================================================
//  Squidex Headless CMS
// ================================ ==========================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using GraphQL;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Text;
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
            content = TestContent.Create(schemaId, contentId, Guid.NewGuid(), Guid.NewGuid(), null);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored))
                .Returns(commandContext);
        }

        [Fact]
        public async Task Should_return_single_content_when_creating_content()
        {
            var query = @"
                mutation OP($data: MySchemaDataInputDto!) {
                  createMySchemaContent(data: $data, expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var inputContent = GetInputContent(content);
            var inputs = new Inputs(new Dictionary<string, object>
            {
                ["data"] = inputContent
            });

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query, Inputs = inputs });

            var expected = new
            {
                data = new
                {
                    createMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_single_content_when_updating_content()
        {
            var query = @"
                mutation OP($data: MySchemaInputDto!) {
                  updateMySchemaContent(id: ""<ID>"", data: $data, expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var inputContent = GetInputContent(content);
            var inputs = new Inputs(new Dictionary<string, object>
            {
                ["data"] = inputContent
            });

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query, Inputs = inputs });

            var expected = new
            {
                data = new
                {
                    updateMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_single_content_when_patching_content()
        {
            var query = @"
                mutation OP($data: MySchemaInputDto!) {
                  patchMySchemaContent(id: ""{contentId}"", data: $data, expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<FIELDS>", TestContent.AllFields);

            commandContext.Complete(content);

            var inputContent = GetInputContent(content);
            var inputs = new Inputs(new Dictionary<string, object>
            {
                ["data"] = inputContent
            });

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query, Inputs = inputs });

            var expected = new
            {
                data = new
                {
                    patchMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_publish_command_for_status_change()
        {
            var query = @"
                mutation {
                  publishMySchemaContent(id: ""<ID>"", status: ""Published"", expectedVersion: 10) {
                    <FIELDS>
                  }
                }".Replace("<ID>", contentId.ToString()).Replace("<FIELDS>", TestContent.AllFields);

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
                    x.Status == Status.Published &&
                    x.ExpectedVersion == 10)))
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

        private static object GetInputContent(IContentEntity content)
        {
            var camelContent = new Dictionary<string, object?>();

            foreach (var (fieldName, fieldValue) in content.Data)
            {
                var name = CasingExtensions.ToCamelCase(fieldName);

                if (fieldValue != null)
                {
                    var fieldValueResult = new Dictionary<string, object?>();

                    foreach (var (partition, value) in fieldValue)
                    {
                        fieldValueResult[partition] = GetInputContent(value);
                    }

                    camelContent[name] = fieldValue;
                }
                else
                {
                    camelContent[name] = null;
                }
            }

            return camelContent;
        }

        private static object? GetInputContent(IJsonValue? json)
        {
            switch (json)
            {
                case JsonNull _:
                    return null;
                case JsonNumber n:
                    return n.Value;
                case JsonString s:
                    return s.Value;
                case JsonArray a:
                    return a.Select(x => GetInputContent(x)).ToList();
                case JsonObject o:
                    return o.ToDictionary(x => x.Key, x => GetInputContent(x.Value));
                default:
                    return null;
            }
        }
    }
}
