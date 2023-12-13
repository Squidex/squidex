// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public class GraphQLQueriesTests : GraphQLTestBase
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_return_error_empty_query(string query)
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = query
        });

        var expected = new
        {
            errors = new object[]
            {
                new
                {
                    message = "Document does not contain any operations.",
                    extensions = new
                    {
                        code = "NO_OPERATION",
                        codes = new[]
                        {
                            "NO_OPERATION"
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_contents_with_full_text()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$skip=0&$search=\"Hello\"" && x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContents(search: 'Hello') {
                    {fields}
                  }
                }",
            Args = new
            {
                fields = TestContent.AllFlatFields
            }
        });

        var expected = new
        {
            data = new
            {
                queryMySchemaContents = new[]
                {
                    TestContent.FlatResponse(content)
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_contents_with_ids()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIds(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryContentsByIds(ids: ['{contentId}']) {
                    ... on Content {
                      id
                    }
                    ... on MySchema {
                      flatData {
                        myNumber
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                queryContentsByIds = new[]
                {
                    new
                    {
                        id = contentId,
                        flatData = new
                        {
                            myNumber = 1
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_contents_with_ids_and_dynamic_data()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIds(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryContentsByIds(ids: ['{contentId}']) {
                    ... on Content {
                      data: data__dynamic
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                queryContentsByIds = new[]
                {
                    new
                    {
                        data = content.Data
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_assets()
    {
        var asset = TestAsset.Create(DomainId.NewGuid());

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5&$filter=my-query" && x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, asset));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryAssets(filter: 'my-query', top: 30, skip: 5) {
                    {fields}
                  }
                }",
            Args = new
            {
                fields = TestAsset.AllFields
            }
        });

        var expected = new
        {
            data = new
            {
                queryAssets = new[]
                {
                    TestAsset.Response(asset)
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_assets_with_total()
    {
        var asset = TestAsset.Create(DomainId.NewGuid());

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5&$filter=my-query" && !x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, asset));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryAssetsWithTotal(filter: 'my-query', top: 30, skip: 5) {
                    total
                    items {
                      {fields}
                    }
                  }
                }",
            Args = new
            {
                fields = TestAsset.AllFields
            }
        });

        var expected = new
        {
            data = new
            {
                queryAssetsWithTotal = new
                {
                    total = 10,
                    items = new[]
                    {
                        TestAsset.Response(asset)
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_return_null_if_single_asset_not_found()
    {
        var assetId = DomainId.NewGuid();

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom<EnrichedAsset>(1));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findAsset(id: '{assetId}') {
                    id,
                    version
                  }
                }",
            Args = new
            {
                assetId
            }
        });

        var expected = new
        {
            data = new
            {
                findAsset = (object?)null
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_find_single_asset()
    {
        var assetId = DomainId.NewGuid();
        var asset = TestAsset.Create(assetId);

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, asset));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findAsset(id: '{assetId}') {
                    {fields}
                  }
                }",
            Args = new
            {
                assetId,
                fields = TestAsset.AllFields
            }
        });

        var expected = new
        {
            data = new
            {
                findAsset = TestAsset.Response(asset)
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_contents_with_flat_data()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContents(top: 30, skip: 5) {
                    {fields}
                  }
                }",
            Args = new
            {
                fields = TestContent.AllFlatFields
            }
        });

        var expected = new
        {
            data = new
            {
                queryMySchemaContents = new[]
                {
                    TestContent.FlatResponse(content)
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_contents()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContents(top: 30, skip: 5) {
                    {fields}
                  }
                }",
            Args = new
            {
                fields = TestContent.AllFields
            }
        });

        var expected = new
        {
            data = new
            {
                queryMySchemaContents = new[]
                {
                    TestContent.Response(content)
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_query_contents_with_total()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && !x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContentsWithTotal(top: 30, skip: 5) {
                    total
                    items {
                      {fields}
                    }
                  }
                }",
            Args = new
            {
                fields = TestContent.AllFields
            }
        });

        var expected = new
        {
            data = new
            {
                queryMySchemaContentsWithTotal = new
                {
                    total = 10,
                    items = new[]
                    {
                        TestContent.Response(content)
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_return_null_if_single_content_not_found()
    {
        var contentId = DomainId.NewGuid();

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom<EnrichedContent>(1));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id,
                    version
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = (object?)null
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_return_null_if_single_content_from_another_schema()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentId, "reference1-field", "reference1");

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id,
                    version
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = (object?)null
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_find_single_content()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_find_single_content_with_version()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.FindAsync(MatchsContentContext(), content.SchemaId.Id.ToString(), contentId, 3,
                A<CancellationToken>._))
            .Returns(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}', version: 3) {
                    {fields}
                  }
                }",
            Args = new
            {
                contentId,
                fields = TestContent.AllFields
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = TestContent.Response(content)
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_find_singleton_content()
    {
        var contentId = TestSchemas.Singleton.Id;
        var content = TestContent.CreateSimple(TestSchemas.Singleton.NamedId(), contentId, "singleton-field", "Hello");

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySingletonSingleton {
                    id,
                    flatData {
                      singletonField
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySingletonSingleton = new
                {
                    id = contentId,
                    flatData = new
                    {
                        singletonField = "Hello"
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_find_singleton_content_with_version()
    {
        var contentId = TestSchemas.Singleton.Id;
        var content = TestContent.CreateSimple(TestSchemas.Singleton.NamedId(), contentId, "singleton-field", "Hello");

        A.CallTo(() => contentQuery.FindAsync(MatchsContentContext(), content.SchemaId.Id.ToString(), contentId, 3,
                A<CancellationToken>._))
            .Returns(content);

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySingletonSingleton(version: 3) {
                    id,
                    flatData {
                      singletonField
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySingletonSingleton = new
                {
                    id = contentId,
                    flatData = new
                    {
                        singletonField = "Hello"
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_embedded_contents()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-embeds",
                     new ContentFieldData()
                        .AddInvariant(JsonValue.Create($"contents:{contentRefId}")));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    data {
                      myEmbeds {
                        iv {
                          text
                          contents {
                            ... on Content {
                              id
                            }
                            ... on MyReference1 {
                              data {
                                reference1Field {
                                  iv
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    data = new
                    {
                        myEmbeds = new
                        {
                            iv = new
                            {
                                text = $"contents:{contentRefId}",
                                contents = new[]
                                {
                                    new
                                    {
                                        id = contentRefId,
                                        data = new
                                        {
                                            reference1Field = new
                                            {
                                                iv = "reference1"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_richtext_contents()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-richtext",
                     new ContentFieldData()
                        .AddInvariant(JsonValue.Object()
                            .Add("type", "text")
                            .Add("marks", JsonValue.Array(
                                JsonValue.Object()
                                    .Add("type", "link")
                                    .Add("attrs", JsonValue.Object()
                                        .Add("href", $"contents:{contentRefId}"))))));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    data {
                      myRichtext {
                        iv {
                          value
                          contents {
                            ... on Content {
                              id
                            }
                            ... on MyReference1 {
                              data {
                                reference1Field {
                                  iv
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    data = new
                    {
                        myRichtext = new
                        {
                            iv = new
                            {
                                value = new
                                {
                                    type = "text",
                                    marks = new[]
                                    {
                                        new
                                        {
                                            type = "link",
                                            attrs = new
                                            {
                                                href = $"contents:{contentRefId}"
                                            }
                                        }
                                    }
                                },
                                contents = new[]
                                {
                                    new
                                    {
                                        id = contentRefId,
                                        data = new
                                        {
                                            reference1Field = new
                                            {
                                                iv = "reference1"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_referenced_contents()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    data {
                      myReferences {
                        iv {
                          id
                          data {
                            reference1Field {
                              iv
                            }
                          }
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    data = new
                    {
                        myReferences = new
                        {
                            iv = new[]
                            {
                                new
                                {
                                    id = contentRefId,
                                    data = new
                                    {
                                        reference1Field = new
                                        {
                                            iv = "reference1"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_referenced_contents_from_flat_data()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    flatData {
                      myReferences {
                        id
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    flatData = new
                    {
                        myReferences = new[]
                        {
                            new
                            {
                                id = contentRefId
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_cache_referenced_contents_from_flat_data()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var query = new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    flatData {
                      myReferences @cache(duration: 1000) {
                        id
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        };

        var actual1 = await ExecuteAsync(query);
        var actual2 = await ExecuteAsync(query);

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    flatData = new
                    {
                        myReferences = new[]
                        {
                            new
                            {
                                id = contentRefId
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual1);
        AssertResult(expected, actual2);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_resolve_referencing_contents()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)))
                .AddField("my-localized-string",
                    new ContentFieldData()
                        .AddLocalized("de-DE", "de-DE"));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Reference == contentRefId && x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMyReference1Content(id: '{contentId}') {
                    id
                    referencingMySchemaContents(top: 30, skip: 5) {
                      id
                      data {
                        myLocalizedString {
                          de_DE
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId = contentRefId
            }
        });

        var expected = new
        {
            data = new
            {
                findMyReference1Content = new
                {
                    id = contentRefId,
                    referencingMySchemaContents = new[]
                    {
                        new
                        {
                            id = contentId,
                            data = new
                            {
                                myLocalizedString = new
                                {
                                    de_DE = "de-DE"
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_referencing_contents_with_total()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)))
                .AddField("my-localized-string",
                    new ContentFieldData()
                        .AddLocalized("de-DE", "de-DE"));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Reference == contentRefId && !x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMyReference1Content(id: '{contentId}') {
                    id
                    referencingMySchemaContentsWithTotal(top: 30, skip: 5) {
                      total
                      items {
                        id
                        data {
                          myLocalizedString {
                            de_DE
                          }
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId = contentRefId
            }
        });

        var expected = new
        {
            data = new
            {
                findMyReference1Content = new
                {
                    id = contentRefId,
                    referencingMySchemaContentsWithTotal = new
                    {
                        total = 10,
                        items = new[]
                        {
                            new
                            {
                                id = contentId,
                                data = new
                                {
                                    myLocalizedString = new
                                    {
                                        de_DE = "de-DE"
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_references_contents()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), contentRef.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Referencing == contentId && x.NoTotal),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, contentRef));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    referencesMyReference1Contents(top: 30, skip: 5) {
                      id
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = contentId,
                    referencesMyReference1Contents = new[]
                    {
                        new
                        {
                            id = contentRefId
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_references_contents_with_total()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), contentRef.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Referencing == contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, contentRef));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    referencesMyReference1ContentsWithTotal(top: 30, skip: 5) {
                      total
                      items {
                        id
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = contentId,
                    referencesMyReference1ContentsWithTotal = new
                    {
                        total = 10,
                        items = new[]
                        {
                            new
                            {
                                id = contentRefId
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_union_contents()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateSimple(TestSchemas.Reference1.NamedId(), contentRefId, "reference1-field", "reference1");

        var data =
            new ContentData()
                .AddField("my-union",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(contentRefId)));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    data {
                      myUnion {
                        iv {
                          ... on Content {
                            id
                          }
                          ... on MyReference1 {
                            data {
                              reference1Field {
                                iv
                              }
                            }
                          }
                          __typename
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    data = new
                    {
                        myUnion = new
                        {
                            iv = new[]
                            {
                                new
                                {
                                    id = contentRefId,
                                    data = new
                                    {
                                        reference1Field = new
                                        {
                                            iv = "reference1"
                                        }
                                    },
                                    __typename = "MyReference1"
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_embedded_assets()
    {
        var assetRefId = DomainId.NewGuid();
        var assetRef = TestAsset.Create(assetRefId);

        var data =
            new ContentData()
                .AddField("my-embeds",
                     new ContentFieldData()
                        .AddInvariant(JsonValue.Create($"assets:{assetRefId}")));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, assetRef));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    data {
                      myEmbeds {
                        iv {
                          text
                          assets {
                            id
                          }
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    data = new
                    {
                        myEmbeds = new
                        {
                            iv = new
                            {
                                text = $"assets:{assetRefId}",
                                assets = new[]
                                {
                                    new
                                    {
                                        id = assetRefId
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_richtext_assets()
    {
        var assetRefId = DomainId.NewGuid();
        var assetRef = TestAsset.Create(assetRefId);

        var data =
            new ContentData()
                .AddField("my-richtext",
                     new ContentFieldData()
                        .AddInvariant(JsonValue.Object()
                            .Add("type", "image")
                            .Add("attrs", JsonValue.Object()
                                .Add("src", $"assets:{assetRefId}"))));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, assetRef));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    data {
                      myRichtext {
                        iv {
                          value
                          assets {
                            id
                          }
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    data = new
                    {
                        myRichtext = new
                        {
                            iv = new
                            {
                                value = new
                                {
                                    type = "image",
                                    attrs = new
                                    {
                                        src = $"assets:{assetRefId}"
                                    }
                                },
                                assets = new[]
                                {
                                    new
                                    {
                                        id = assetRefId
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_resolve_referenced_assets()
    {
        var assetRefId = DomainId.NewGuid();
        var assetRef = TestAsset.Create(assetRefId);

        var data =
            new ContentData()
                .AddField("my-assets",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(assetRefId)));

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetRefId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, assetRef));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    data {
                      myAssets {
                        iv {
                          id
                        }
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = content.Id,
                    data = new
                    {
                        myAssets = new
                        {
                            iv = new[]
                            {
                                new
                                {
                                    id = assetRefId
                                }
                            }
                        }
                    }
                }
            }
        };

        AssertResult(expected, actual);
    }

    [Fact]
    public async Task Should_not_return_data_if_field_not_part_of_content()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, data: []);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    url
                    data {
                      myInvalid {
                        iv
                      }
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var json = serializer.Serialize(actual);

        Assert.Contains("\"errors\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_query_only_selected_fields()
    {
        await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContents @optimizeFieldQueries {
                    data {
                      myNumber {
                        iv
                      }
                    }
                  }
                }"
        });

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.HasFields(new[] { "my-number" }),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_query_only_selected_flat_fields()
    {
        await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContents @optimizeFieldQueries {
                    flatData {
                      myNumber
                    }
                  }
                }"
        });

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.HasFields(new[] { "my-number" }),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_query_all_fields_when_directive_not_applied()
    {
        await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContents {
                    data {
                      myNumber {
                        iv
                      }
                    }
                  }
                }"
        });

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.Matches(x => x.Fields == null),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_query_all_fields_when_dynamic_data_is_queried()
    {
        await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryMySchemaContents @optimizeFieldQueries {
                    flatData {
                      myNumber
                    }
                    data__dynamic
                  }
                }"
        });

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.Matches(x => x.Fields == null),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_query_all_fields_across_schemas()
    {
        await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  queryContentsByIds(ids: ['42']) @optimizeFieldQueries {
                    ...on MySchema {
                      flatData {
                        myNumber
                      }
                    }
                  }
                }"
        });

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasFields(new[] { "my-number" }),
                A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_fetch_user_if_only_is_id_queried()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query {
                  findMySchemaContent(id: '{contentId}') {
                    createdByUser {
                      id
                    }
                  }
                }",
            Args = new
            {
                contentId
            }
        });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    createdByUser = new
                    {
                        id = content.CreatedBy.Identifier
                    }
                }
            }
        };

        AssertResult(expected, actual);

        A.CallTo(() => userResolver.FindByIdAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
