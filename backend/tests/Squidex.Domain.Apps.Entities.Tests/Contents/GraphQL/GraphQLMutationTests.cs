// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
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
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLMutationTests : GraphQLTestBase
    {
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly IEnrichedContentEntity content;
        private readonly CommandContext commandContext = new CommandContext(new PatchContent(), A.Dummy<ICommandBus>());

        public GraphQLMutationTests()
        {
            content = TestContent.Create(contentId, TestSchemas.Ref1.Id, TestSchemas.Ref2.Id, null);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored))
                .Returns(commandContext);
        }

        [Fact]
        public async Task Should_return_error_if_user_has_no_permission_to_create()
        {
            var query = @"
                mutation {
                  createMySchemaContent(data: { myNumber: { iv: 42 } }) {
                    id
                  }
                }";

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsReadOwn);

            var expected = new
            {
                errors = new[]
                {
                    new
                    {
                        message = "You do not have the necessary permission.",
                        locations = new[]
                        {
                            new
                            {
                                line = 3,
                                column = 19
                            }
                        },
                        path = new[]
                        {
                            "createMySchemaContent"
                        }
                    }
                },
                data = (object?)null
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_creating_content()
        {
            var query = CreateQuery(@"
                mutation {
                  createMySchemaContent(data: <DATA>, publish: true) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsCreate);

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
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_creating_content_with_custom_id()
        {
            var query = CreateQuery(@"
                mutation {
                  createMySchemaContent(data: <DATA>, id: '123', publish: true) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsCreate);

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
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.ContentId == DomainId.Create("123") &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_creating_content_with_variable()
        {
            var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  createMySchemaContent(data: $data, publish: true) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query, Inputs = GetInput() }, Permissions.AppContentsCreate);

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
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_error_if_user_has_no_permission_to_update()
        {
            var query = CreateQuery(@"
                mutation {
                  updateMySchemaContent(id: '<ID>', data: { myNumber: { iv: 42 } }) {
                    id
                  }
                }");

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsReadOwn);

            var expected = new
            {
                errors = new[]
                {
                    new
                    {
                        message = "You do not have the necessary permission.",
                        locations = new[]
                        {
                            new
                            {
                                line = 3,
                                column = 19
                            }
                        },
                        path = new[]
                        {
                            "updateMySchemaContent"
                        }
                    }
                },
                data = (object?)null
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_updating_content()
        {
            var query = CreateQuery(@"
                mutation {
                  updateMySchemaContent(id: '<ID>', data: <DATA>, expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsUpdateOwn);

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
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_updating_content_with_variable()
        {
            var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  updateMySchemaContent(id: '<ID>', data: $data, expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query, Inputs = GetInput() }, Permissions.AppContentsUpdateOwn);

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
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_error_if_user_has_no_permission_to_upsert()
        {
            var query = CreateQuery(@"
                mutation {
                  upsertMySchemaContent(id: '<ID>', data: { myNumber: { iv: 42 } }) {
                    id
                  }
                }");

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsReadOwn);

            var expected = new
            {
                errors = new[]
                {
                    new
                    {
                        message = "You do not have the necessary permission.",
                        locations = new[]
                        {
                            new
                            {
                                line = 3,
                                column = 19
                            }
                        },
                        path = new[]
                        {
                            "upsertMySchemaContent"
                        }
                    }
                },
                data = (object?)null
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_upserting_content()
        {
            var query = CreateQuery(@"
                mutation {
                  upsertMySchemaContent(id: '<ID>', data: <DATA>, publish: true, expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsUpsert);

            var expected = new
            {
                data = new
                {
                    upsertMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_upserting_content_with_variable()
        {
            var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  upsertMySchemaContent(id: '<ID>', data: $data, publish: true, expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query, Inputs = GetInput() }, Permissions.AppContentsUpsert);

            var expected = new
            {
                data = new
                {
                    upsertMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_error_if_user_has_no_permission_to_patch()
        {
            var query = CreateQuery(@"
                mutation {
                  patchMySchemaContent(id: '<ID>', data: { myNumber: { iv: 42 } }) {
                    id
                  }
                }");

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsReadOwn);

            var expected = new
            {
                errors = new[]
                {
                    new
                    {
                        message = "You do not have the necessary permission.",
                        locations = new[]
                        {
                            new
                            {
                                line = 3,
                                column = 19
                            }
                        },
                        path = new[]
                        {
                            "patchMySchemaContent"
                        }
                    }
                },
                data = (object?)null
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_patching_content()
        {
            var query = CreateQuery(@"
                mutation {
                  patchMySchemaContent(id: '<ID>', data: <DATA>, expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsUpdateOwn);

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
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_patching_content_with_variable()
        {
            var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  patchMySchemaContent(id: '<ID>', data: $data, expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query, Inputs = GetInput() }, Permissions.AppContentsUpdateOwn);

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
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_error_if_user_has_no_permission_to_change_status()
        {
            var query = CreateQuery(@"
                mutation {
                  changeMySchemaContent(id: '<ID>', status: 'Published') {
                    id
                  }
                }");

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsReadOwn);

            var expected = new
            {
                errors = new[]
                {
                    new
                    {
                        message = "You do not have the necessary permission.",
                        locations = new[]
                        {
                            new
                            {
                                line = 3,
                                column = 19
                            }
                        },
                        path = new[]
                        {
                            "changeMySchemaContent"
                        }
                    }
                },
                data = (object?)null
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_changing_status()
        {
            var dueTime = InstantPattern.General.Parse("2021-12-12T11:10:09Z").Value;

            var query = CreateQuery(@"
                mutation {
                  changeMySchemaContent(id: '<ID>', status: 'Published', dueTime: '2021-12-12T11:10:09Z', expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsChangeStatusOwn);

            var expected = new
            {
                data = new
                {
                    changeMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.DueTime == dueTime &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_changing_status_without_due_time()
        {
            var query = CreateQuery(@"
                mutation {
                  changeMySchemaContent(id: '<ID>', status: 'Published', expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsChangeStatusOwn);

            var expected = new
            {
                data = new
                {
                    changeMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.DueTime == null &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_single_content_if_changing_status_with_null_due_time()
        {
            var query = CreateQuery(@"
                mutation {
                  changeMySchemaContent(id: '<ID>', status: 'Published', dueTime: null, expectedVersion: 10) {
                    <FIELDS>
                  }
                }");

            commandContext.Complete(content);

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsChangeStatusOwn);

            var expected = new
            {
                data = new
                {
                    changeMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.DueTime == null &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_error_if_user_has_no_permission_to_delete()
        {
            var query = CreateQuery(@"
                mutation {
                  deleteMySchemaContent(id: '<ID>') {
                    version
                  }
                }");

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsReadOwn);

            var expected = new
            {
                errors = new[]
                {
                    new
                    {
                        message = "You do not have the necessary permission.",
                        locations = new[]
                        {
                            new
                            {
                                line = 3,
                                column = 19
                            }
                        },
                        path = new[]
                        {
                            "deleteMySchemaContent"
                        }
                    }
                },
                data = (object?)null
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_new_version_if_deleting_content()
        {
            var query = CreateQuery(@"
                mutation {
                  deleteMySchemaContent(id: '<ID>', expectedVersion: 10) {
                    version 
                  }
                }");

            commandContext.Complete(CommandResult.Empty(contentId, 13, 12));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query }, Permissions.AppContentsDeleteOwn);

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
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId))))
                .MustHaveHappened();
        }

        private string CreateQuery(string query)
        {
            query = query
                .Replace("<ID>", contentId.ToString(), StringComparison.Ordinal)
                .Replace("'", "\"", StringComparison.Ordinal)
                .Replace("`", "\"", StringComparison.Ordinal)
                .Replace("<FIELDS>", TestContent.AllFields, StringComparison.Ordinal);

            if (query.Contains("<DATA>", StringComparison.Ordinal))
            {
                var data = TestContent.Input(content, TestSchemas.Ref1.Id, TestSchemas.Ref2.Id);

                var dataJson = JsonConvert.SerializeObject(data, Formatting.Indented);
                var dataString = Regex.Replace(dataJson, "\"([^\"]+)\":", x => x.Groups[1].Value + ":").Replace(".0", string.Empty, StringComparison.Ordinal);

                query = query.Replace("<DATA>", dataString, StringComparison.Ordinal);
            }

            return query;
        }

        private Inputs GetInput()
        {
            var input = new
            {
                data = TestContent.Input(content, TestSchemas.Ref1.Id, TestSchemas.Ref2.Id)
            };

            return JObject.FromObject(input).ToInputs();
        }
    }
}
