// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public class GraphQLMutationTests : GraphQLTestBase
{
    private readonly DomainId contentId = DomainId.NewGuid();
    private readonly EnrichedContent content;
    private readonly CommandContext commandContext = new CommandContext(new PatchContent(), A.Dummy<ICommandBus>());

    public GraphQLMutationTests()
    {
        content = TestContent.Create(contentId);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored, A<CancellationToken>._))
            .Returns(commandContext);
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permission_to_create()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  createMySchemaContent(data: { }) {
                    id
                  }
                }"
        });

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
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN"
                        }
                    }
                }
            },
            data = new
            {
                createMySchemaContent = (object?)null
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_creating_content()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation MyMutation($data: MySchemaDataInputDto!) {
                  createMySchemaContent(data: $data, publish: true) {
                    {fields}
                  }
                }",
            Args = new
            {
                fields = TestContent.AllFields
            },
            Variables = new
            {
                data = TestContent.Input(content),
            },
            Permission = PermissionIds.AppContentsCreate
        });

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
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_create_content_with_variable()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation MyMutation($location: JsonScalar) {
                  createMySchemaContent(data: {
                    myGeolocation: {
                     iv: $location
                    }
                  }) {
                    id
                  }
                }",
            Args = new
            {
                fields = TestContent.AllFields
            },
            Variables = new
            {
                location = new
                {
                    latitude = 42,
                    longitude = 13
                }
            },
            Permission = PermissionIds.AppContentsCreate
        });

        var expected = new
        {
            data = new
            {
                createMySchemaContent = new
                {
                    id = content.Id,
                }
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<CreateContent>.That.Matches(x =>
                    x.ExpectedVersion == EtagVersion.Any &&
                    x.SchemaId.Equals(TestSchemas.Default.NamedId())),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_creating_content_with_custom_id()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation MyMutation($data: MySchemaDataInputDto!) {
                  createMySchemaContent(data: $data, id: '{contentId}', publish: true) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            },
            Variables = new
            {
                data = TestContent.Input(content)
            },
            Permission = PermissionIds.AppContentsCreate
        });

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
                    x.ContentId == contentId &&
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permission_to_update()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  updateMySchemaContent(id: '{contentId}', data: { }) {
                    id
                  }
                }",
            Args = new
            {
                contentId
            }
        });

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
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN"
                        }
                    }
                }
            },
            data = new
            {
                updateMySchemaContent = (object?)null
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_updating_content()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation MyMutation($data: MySchemaDataInputDto!) {
                  updateMySchemaContent(id: '{contentId}', data: $data, expectedVersion: 10) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            },
            Variables = new
            {
                data = TestContent.Input(content)
            },
            Permission = PermissionIds.AppContentsUpdateOwn
        });

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
                    x.ContentId == contentId &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permission_to_upsert()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  upsertMySchemaContent(id: '{contentId}', data: { }) {
                    id
                  }
                }"
        });

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
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN"
                        }
                    }
                }
            },
            data = new
            {
                upsertMySchemaContent = (object?)null
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_upserting_content()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation MyMutation($data: MySchemaDataInputDto!) {
                  upsertMySchemaContent(id: '{contentId}', data: $data, publish: true, expectedVersion: 10) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            },
            Variables = new
            {
                data = TestContent.Input(content)
            },
            Permission = PermissionIds.AppContentsUpsert
        });

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
                    x.ContentId == contentId &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Status == Status.Published &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permission_to_patch()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  patchMySchemaContent(id: '{contentId}', data: { }) {
                    id
                  }
                }",
            Args = new
            {
                contentId
            },
        });

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
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN"
                        }
                    }
                }
            },
            data = new
            {
                patchMySchemaContent = (object?)null
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_patching_content()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation MyMutation($data: MySchemaDataInputDto!) {
                  patchMySchemaContent(id: '{contentId}', data: $data, expectedVersion: 10) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            },
            Variables = new
            {
                data = TestContent.Input(content)
            },
            Permission = PermissionIds.AppContentsUpdateOwn
        });

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
                    x.ContentId == contentId &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Data.Equals(content.Data)),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permission_to_change_status()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  changeMySchemaContent(id: '{contentId}', status: 'Published') {
                    id
                  }
                }",
            Args = new
            {
                contentId
            },
        });

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
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN"
                        }
                    }
                }
            },
            data = new
            {
                changeMySchemaContent = (object?)null
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(A<ICommand>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_changing_status()
    {
        var dueTime = InstantPattern.General.Parse("2021-12-12T11:10:09Z").Value;

        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  changeMySchemaContent(id: '{contentId}', status: 'Published', dueTime: '2021-12-12T11:10:09Z', expectedVersion: 10) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            },
            Permission = PermissionIds.AppContentsChangeStatusOwn
        });

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
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Status == Status.Published),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_changing_status_without_due_time()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  changeMySchemaContent(id: '{contentId}', status: 'Published', expectedVersion: 10) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            },
            Permission = PermissionIds.AppContentsChangeStatusOwn
        });

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
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Status == Status.Published),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_single_content_if_changing_status_with_null_due_time()
    {
        commandContext.Complete(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  changeMySchemaContent(id: '{contentId}', status: 'Published', dueTime: null, expectedVersion: 10) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            },
            Permission = PermissionIds.AppContentsChangeStatusOwn
        });

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
                    x.SchemaId.Equals(TestSchemas.Default.NamedId()) &&
                    x.Status == Status.Published),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_error_if_user_has_no_permission_to_delete()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  deleteMySchemaContent(id: '{contentId}') {
                    version
                  }
                }",
            Args = new
            {
                contentId
            },
        });

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
                    },
                    extensions = new
                    {
                        code = "DOMAIN_FORBIDDEN",
                        codes = new[]
                        {
                            "DOMAIN_FORBIDDEN"
                        }
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
        commandContext.Complete(CommandResult.Empty(contentId, 1, 0));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                mutation {
                  deleteMySchemaContent(id: '{contentId}', expectedVersion: 10) {
                    version
                  }
                }",
            Args = new
            {
                contentId
            },
            Permission = PermissionIds.AppContentsDeleteOwn
        });

        var expected = new
        {
            data = new
            {
                deleteMySchemaContent = new
                {
                    version = 1
                }
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => commandBus.PublishAsync(
                A<DeleteContent>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.ExpectedVersion == 10 &&
                    x.SchemaId.Equals(TestSchemas.Default.NamedId())),
                A<CancellationToken>._))
            .MustHaveHappened();
    }
}
