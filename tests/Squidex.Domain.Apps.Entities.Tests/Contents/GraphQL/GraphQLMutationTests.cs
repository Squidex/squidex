// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json.Linq;
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
        private readonly IContentEntity content;
        private readonly CommandContext commandContext = new CommandContext(new PatchContent(), A.Dummy<ICommandBus>());

        public GraphQLMutationTests()
        {
            content = CreateContent(contentId, Guid.Empty, Guid.Empty, null, true);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored))
                .Returns(commandContext);
        }

        [Fact]
        public async Task Should_return_single_content_when_creating_content()
        {
            var query = $@"
                mutation OP($data: MySchemaInputDto!) {{
                  createMySchemaContent(data: $data, expectedVersion: 10) {{
                    version
                    data {{
                      myString {{
                        de
                      }}
                      myNumber {{
                        iv
                      }}
                      myBoolean {{
                        iv
                      }}
                      myDatetime {{
                        iv
                      }}
                      myGeolocation {{
                        iv
                      }}
                      myTags {{
                        iv
                      }}
                    }}
                  }}
                }}";

            commandContext.Complete(new EntityCreatedResult<NamedContentData>(content.Data, 13));

            var inputContent = GetInputContent(content);

            var variables =
                new JObject(
                    new JProperty("data", inputContent));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query, Variables = variables });

            var expected = new
            {
                data = new
                {
                    createMySchemaContent = new
                    {
                        version = 13,
                        data = new
                        {
                            myString = new
                            {
                                de = "value"
                            },
                            myNumber = new
                            {
                                iv = 1
                            },
                            myBoolean = new
                            {
                                iv = true
                            },
                            myDatetime = new
                            {
                                iv = content.LastModified.ToDateTimeUtc()
                            },
                            myGeolocation = new
                            {
                                iv = new
                                {
                                    latitude = 10,
                                    longitude = 20
                                }
                            },
                            myTags = new
                            {
                                iv = new[]
                                {
                                    "tag1",
                                    "tag2"
                                }
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_single_content_when_updating_content()
        {
            var query = $@"
                mutation OP($data: MySchemaInputDto!) {{
                  updateMySchemaContent(id: ""{contentId}"", data: $data, expectedVersion: 10) {{
                    version
                    data {{
                      myString {{
                        de
                      }}
                      myNumber {{
                        iv
                      }}
                      myBoolean {{
                        iv
                      }}
                      myDatetime {{
                        iv
                      }}
                      myGeolocation {{
                        iv
                      }}
                      myTags {{
                        iv
                      }}
                    }}
                  }}
                }}";

            commandContext.Complete(new ContentDataChangedResult(content.Data, 13));

            var inputContent = GetInputContent(content);

            var variables =
                new JObject(
                    new JProperty("data", inputContent));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query, Variables = variables });

            var expected = new
            {
                data = new
                {
                    updateMySchemaContent = new
                    {
                        version = 13,
                        data = new
                        {
                            myString = new
                            {
                                de = "value"
                            },
                            myNumber = new
                            {
                                iv = 1
                            },
                            myBoolean = new
                            {
                                iv = true
                            },
                            myDatetime = new
                            {
                                iv = content.LastModified.ToDateTimeUtc()
                            },
                            myGeolocation = new
                            {
                                iv = new
                                {
                                    latitude = 10,
                                    longitude = 20
                                }
                            },
                            myTags = new
                            {
                                iv = new[]
                                {
                                    "tag1",
                                    "tag2"
                                }
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_single_content_when_patching_content()
        {
            var query = $@"
                mutation OP($data: MySchemaInputDto!) {{
                  patchMySchemaContent(id: ""{contentId}"", data: $data, expectedVersion: 10) {{
                    version
                    data {{
                      myString {{
                        de
                      }}
                      myNumber {{
                        iv
                      }}
                      myBoolean {{
                        iv
                      }}
                      myDatetime {{
                        iv
                      }}
                      myGeolocation {{
                        iv
                      }}
                      myTags {{
                        iv
                      }}
                    }}
                  }}
                }}";

            commandContext.Complete(new ContentDataChangedResult(content.Data, 13));

            var inputContent = GetInputContent(content);

            var variables =
                new JObject(
                    new JProperty("data", inputContent));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query, Variables = variables });

            var expected = new
            {
                data = new
                {
                    patchMySchemaContent = new
                    {
                        version = 13,
                        data = new
                        {
                            myString = new
                            {
                                de = "value"
                            },
                            myNumber = new
                            {
                                iv = 1
                            },
                            myBoolean = new
                            {
                                iv = true
                            },
                            myDatetime = new
                            {
                                iv = content.LastModified.ToDateTimeUtc()
                            },
                            myGeolocation = new
                            {
                                iv = new
                                {
                                    latitude = 10,
                                    longitude = 20
                                }
                            },
                            myTags = new
                            {
                                iv = new[]
                                {
                                    "tag1",
                                    "tag2"
                                }
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_publish_command_for_publish()
        {
            var query = $@"
                mutation {{
                  publishMySchemaContent(id: ""{contentId}"", expectedVersion: 10) {{
                    version
                  }}
                }}";

            commandContext.Complete(new EntitySavedResult(13));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    publishMySchemaContent = new
                    {
                        version = 13
                    }
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
        public async Task Should_publish_command_for_unpublish()
        {
            var query = $@"
                mutation {{
                  unpublishMySchemaContent(id: ""{contentId}"", expectedVersion: 10) {{
                    version
                  }}
                }}";

            commandContext.Complete(new EntitySavedResult(13));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    unpublishMySchemaContent = new
                    {
                        version = 13
                    }
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.Status == Status.Draft &&
                    x.ExpectedVersion == 10)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_archive()
        {
            var query = $@"
                mutation {{
                  archiveMySchemaContent(id: ""{contentId}"", expectedVersion: 10) {{
                    version
                  }}
                }}";

            commandContext.Complete(new EntitySavedResult(13));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    archiveMySchemaContent = new
                    {
                        version = 13
                    }
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.Status == Status.Archived &&
                    x.ExpectedVersion == 10)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_restore()
        {
            var query = $@"
                mutation {{
                  restoreMySchemaContent(id: ""{contentId}"", expectedVersion: 10) {{
                    version
                  }}
                }}";

            commandContext.Complete(new EntitySavedResult(13));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    restoreMySchemaContent = new
                    {
                        version = 13
                    }
                }
            };

            AssertResult(expected, result);

            A.CallTo(() => commandBus.PublishAsync(
                A<ChangeContentStatus>.That.Matches(x =>
                    x.ContentId == contentId &&
                    x.Status == Status.Draft &&
                    x.ExpectedVersion == 10)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_delete()
        {
            var query = $@"
                mutation {{
                  deleteMySchemaContent(id: ""{contentId}"", expectedVersion: 10) {{
                    version 
                  }}
                }}";

            commandContext.Complete(new EntitySavedResult(13));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query });

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

        private static JObject GetInputContent(IContentEntity content)
        {
            var camelContent = new NamedContentData();

            foreach (var kvp in content.Data)
            {
                camelContent[kvp.Key.ToCamelCase()] = kvp.Value;
            }

            return JObject.FromObject(camelContent);
        }
    }
}
