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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLMutationTests : GraphQLTestBase
    {
        private readonly CommandContext commandContext = new CommandContext(new PatchContent());

        public GraphQLMutationTests()
        {
            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.Ignored))
                .Returns(commandContext);
        }

        [Fact]
        public async Task Should_return_single_content_when_patching_content()
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, Guid.Empty);

            var query = $@"
                mutation OP($data: MySchemaInputDto!) {{
                  patchMySchemaContent(id: ""{contentId}"", data: $data) {{
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
                    myJson {{
                      iv
                    }}
                    myGeolocation {{
                      iv
                    }}
                    myTags {{
                      iv
                    }}
                  }}
                }}";

            commandContext.Complete(new ContentDataChangedResult(content.Data, 1));

            var camelContent = new NamedContentData();

            foreach (var kvp in content.Data)
            {
                if (kvp.Key != "my-json")
                {
                    camelContent[kvp.Key.ToCamelCase()] = kvp.Value;
                }
            }

            var variables =
                new JObject(
                    new JProperty("data", JObject.FromObject(camelContent)));

            var result = await sut.QueryAsync(app, user, new GraphQLQuery { Query = query, Variables = variables });

            var expected = new
            {
                data = new
                {
                    patchMySchemaContent = new
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
                        myJson = new
                        {
                            iv = new
                            {
                                value = 1
                            }
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
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_publish_command_for_publish()
        {
            var contentId = Guid.NewGuid();

            var query = $@"
                mutation {{
                  publishMySchemaContent(id: ""{contentId}"") {{
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
                    x.SchemaId.Equals(schema.NamedId()) &&
                    x.ContentId == contentId &&
                    x.Status == Status.Published)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_unpublish()
        {
            var contentId = Guid.NewGuid();

            var query = $@"
                mutation {{
                  unpublishMySchemaContent(id: ""{contentId}"") {{
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
                    x.SchemaId.Equals(schema.NamedId()) &&
                    x.ContentId == contentId &&
                    x.Status == Status.Draft)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_archive()
        {
            var contentId = Guid.NewGuid();

            var query = $@"
                mutation {{
                  archiveMySchemaContent(id: ""{contentId}"") {{
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
                    x.SchemaId.Equals(schema.NamedId()) &&
                    x.ContentId == contentId &&
                    x.Status == Status.Archived)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_restore()
        {
            var contentId = Guid.NewGuid();

            var query = $@"
                mutation {{
                  restoreMySchemaContent(id: ""{contentId}"") {{
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
                    x.SchemaId.Equals(schema.NamedId()) &&
                    x.ContentId == contentId &&
                    x.Status == Status.Draft)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_command_for_delete()
        {
            var contentId = Guid.NewGuid();

            var query = $@"
                mutation {{
                  deleteMySchemaContent(id: ""{contentId}"") {{
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
                    x.SchemaId.Equals(schema.NamedId()) &&
                    x.ContentId == contentId)))
                .MustHaveHappened();
        }
    }
}
