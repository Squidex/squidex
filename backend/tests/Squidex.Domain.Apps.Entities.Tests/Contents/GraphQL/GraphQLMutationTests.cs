// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using GraphQL;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public class GraphQLMutationTests : GraphQLTestBase
{
    private readonly DomainId contentId = DomainId.NewGuid();
    private readonly IEnrichedContentEntity content;
    private readonly CommandContext commandContext = new CommandContext(new PatchContent(), A.Dummy<ICommandBus>());

    public GraphQLMutationTests()
    {
        content = TestContent.Create(contentId, TestSchemas.Ref1.Id, TestSchemas.Ref2.Id, null);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored, A<CancellationToken>._))
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

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_creating_content()
    {
        var query = CreateQuery(@"
                mutation {
                  createMySchemaContent(data: <DATA>, publish: true) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsCreate;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                createMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x =>
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_creating_content_with_custom_id()
    {
        var query = CreateQuery(@"
                mutation {
                  createMySchemaContent(data: <DATA>, id: '123', publish: true) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsCreate;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                createMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x =>
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.ContentId == DomainId.Create("123") &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_creating_content_with_variable()
    {
        var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  createMySchemaContent(data: $data, publish: true) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsCreate;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query, Variables = GetInput() }, permission);

        var expected = new
        {
            data = new
            {
                createMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x =>
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
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
                }", contentId, content);

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_updating_content()
    {
        var query = CreateQuery(@"
                mutation {
                  updateMySchemaContent(id: '<ID>', data: <DATA>, expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsUpdateOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                updateMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpdateContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_updating_content_with_variable()
    {
        var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  updateMySchemaContent(id: '<ID>', data: $data, expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsUpdateOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query, Variables = GetInput() }, permission);

        var expected = new
        {
            data = new
            {
                updateMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpdateContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
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
                }", contentId, content);

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_upserting_content()
    {
        var query = CreateQuery(@"
                mutation {
                  upsertMySchemaContent(id: '<ID>', data: <DATA>, publish: true, expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsUpsert;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                upsertMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_upserting_content_with_variable()
    {
        var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  upsertMySchemaContent(id: '<ID>', data: $data, publish: true, expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsUpsert;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query, Variables = GetInput() }, permission);

        var expected = new
        {
            data = new
            {
                upsertMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<UpsertContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
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
                }", contentId, content);

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_patching_content()
    {
        var query = CreateQuery(@"
                mutation {
                  patchMySchemaContent(id: '<ID>', data: <DATA>, expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsUpdateOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                patchMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<PatchContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_patching_content_with_variable()
    {
        var query = CreateQuery(@"
                mutation OP($data: MySchemaDataInputDto!) {
                  patchMySchemaContent(id: '<ID>', data: $data, expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsUpdateOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query, Variables = GetInput() }, permission);

        var expected = new
        {
            data = new
            {
                patchMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<PatchContent>.That.Matches(x =>
                    x.ContentId == content.Id &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
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
                }", contentId, content);

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_changing_status()
    {
        var dueTime = InstantPattern.General.Parse("2021-12-12T11:10:09Z").Value;

        var query = CreateQuery(@"
                mutation {
                  changeMySchemaContent(id: '<ID>', status: 'Published', dueTime: '2021-12-12T11:10:09Z', expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsChangeStatusOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                changeMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.DueTime == dueTime &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_changing_status_without_due_time()
    {
        var query = CreateQuery(@"
                mutation {
                  changeMySchemaContent(id: '<ID>', status: 'Published', expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsChangeStatusOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                changeMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.DueTime == null &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_changing_status_with_null_due_time()
    {
        var query = CreateQuery(@"
                mutation {
                  changeMySchemaContent(id: '<ID>', status: 'Published', dueTime: null, expectedVersion: 10) {
                    <FIELDS_CONTENT>
                  }
                }", contentId, content);

        commandContext.Complete(content);

        var permission = PermissionIds.AppContentsChangeStatusOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

        var expected = new
        {
            data = new
            {
                changeMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.DueTime == null &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId) &&
                    x.Status == Status.Published),
                A<CancellationToken>._))
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
                }", contentId, content);

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
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
                }", contentId, content);

        commandContext.Complete(CommandResult.Empty(contentId, 13, 12));

        var permission = PermissionIds.AppContentsDeleteOwn;

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query }, permission);

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

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<DeleteContent>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.DefaultId)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    private Inputs GetInput()
    {
        var input = new
        {
            data = TestContent.Input(content, TestSchemas.Ref1.Id, TestSchemas.Ref2.Id)
        };

        var element = JsonSerializer.SerializeToElement(input, TestUtils.DefaultOptions());

        return serializer.ReadNode<Inputs>(element)!;
    }
}
