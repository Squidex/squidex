// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public class GraphQLQueriesTests : GraphQLTestBase
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Should_return_error_empty_query(string query)
    {
        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
        var query = CreateQuery(@"
                query {
                  queryMySchemaContents(search: ""Hello"") {
                    <FIELDS_CONTENT_FLAT>
                  }
                }");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$skip=0&$search=\"Hello\"" && x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_multiple_assets_if_querying_assets()
    {
        var query = CreateQuery(@"
                query {
                  queryAssets(filter: 'my-query', top: 30, skip: 5) {
                    <FIELDS_ASSET>
                  }
                }");

        var asset = TestAsset.Create(DomainId.NewGuid());

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5&$filter=my-query" && x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, asset));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_multiple_assets_with_total_if_querying_assets_with_total()
    {
        var query = CreateQuery(@"
                query {
                  queryAssetsWithTotal(filter: 'my-query', top: 30, skip: 5) {
                    total
                    items {
                      <FIELDS_ASSET>
                    }
                  }
                }");

        var asset = TestAsset.Create(DomainId.NewGuid());

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5&$filter=my-query" && !x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, asset));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        var query = CreateQuery(@"
                query {
                  findAsset(id: '<ID>') {
                    id
                  }
                }", assetId);

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom<IEnrichedAssetEntity>(1));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_single_asset_if_finding_asset()
    {
        var assetId = DomainId.NewGuid();
        var asset = TestAsset.Create(assetId);

        var query = CreateQuery(@"
                query {
                  findAsset(id: '<ID>') {
                    <FIELDS_ASSET>
                  }
                }", assetId);

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, asset));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_multiple_flat_contents_if_querying_contents()
    {
        var query = CreateQuery(@"
                query {
                  queryMySchemaContents(top: 30, skip: 5) {
                    <FIELDS_CONTENT_FLAT>
                  }
                }");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_multiple_contents_if_querying_contents()
    {
        var query = CreateQuery(@"
                query {
                  queryMySchemaContents(top: 30, skip: 5) {
                    <FIELDS_CONTENT>
                  }
                }");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_multiple_contents_with_total_if_querying_contents_with_total()
    {
        var query = CreateQuery(@"
                query {
                  queryMySchemaContentsWithTotal(top: 30, skip: 5) {
                    total
                    items {
                      <FIELDS_CONTENT>
                    }
                  }
                }");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && !x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom<IEnrichedContentEntity>(1));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
        var content = TestContent.CreateRef(TestSchemas.Ref1Id, contentId, "ref1-field", "ref1");

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_single_content_if_finding_content()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    <FIELDS_CONTENT>
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_return_single_content_if_finding_content_with_version()
    {
        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>', version: 3) {
                    <FIELDS_CONTENT>
                  }
                }", contentId);

        A.CallTo(() => contentQuery.FindAsync(MatchsContentContext(), TestSchemas.Default.Id.ToString(), contentId, 3, A<CancellationToken>._))
            .Returns(content);

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_also_fetch_embedded_contents_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "schemaRef1Field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    data {
                      myEmbeds {
                        iv {
                          text
                          contents {
                            ... on Content {
                              id
                            }
                            ... on MyRefSchema1 {
                              data {
                                schemaRef1Field {
                                  iv
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
                                text = $"assets:{DomainId.Empty}, contents:{contentRefId}",
                                contents = new[]
                                {
                                    new
                                    {
                                        id = contentRefId,
                                        data = new
                                        {
                                            schemaRef1Field = new
                                            {
                                                iv = "ref1"
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
    public async Task Should_also_fetch_referenced_contents_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "schemaRef1Field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    data {
                      myReferences {
                        iv {
                          id
                          data {
                            schemaRef1Field {
                              iv
                            }
                          }
                        }
                      }
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
                                        schemaRef1Field = new
                                        {
                                            iv = "ref1"
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
    public async Task Should_also_fetch_referenced_contents_from_flat_data_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "schemaRef1Field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    flatData {
                      myReferences {
                        id
                      }
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
    public async Task Should_cache_referenced_contents_from_flat_data_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "schemaRef1Field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    flatData {
                      myReferences @cache(duration: 1000) {
                        id
                      }
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual1 = await ExecuteAsync(new ExecutionOptions { Query = query });
        var actual2 = await ExecuteAsync(new ExecutionOptions { Query = query });

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
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_also_fetch_referencing_contents_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "ref1-field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMyRefSchema1Content(id: '<ID>') {
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
                }", contentRefId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Reference == contentRefId && x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

        var expected = new
        {
            data = new
            {
                findMyRefSchema1Content = new
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
    public async Task Should_also_fetch_referencing_contents_with_total_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "ref1-field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMyRefSchema1Content(id: '<ID>') {
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
                }", contentRefId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Reference == contentRefId && !x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

        var expected = new
        {
            data = new
            {
                findMyRefSchema1Content = new
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
    public async Task Should_also_fetch_references_contents_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "ref1-field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    referencesMyRefSchema1Contents(top: 30, skip: 5) {
                      id
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), contentRef.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Referencing == contentId && x.NoTotal), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, contentRef));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = contentId,
                    referencesMyRefSchema1Contents = new[]
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
    public async Task Should_also_fetch_references_contents_with_total_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "ref1-field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    referencesMyRefSchema1ContentsWithTotal(top: 30, skip: 5) {
                      total
                      items {
                        id
                      }
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), contentRef.SchemaId.Id.ToString(),
                A<Q>.That.Matches(x => x.QueryAsOdata == "?$top=30&$skip=5" && x.Referencing == contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(10, contentRef));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

        var expected = new
        {
            data = new
            {
                findMySchemaContent = new
                {
                    id = contentId,
                    referencesMyRefSchema1ContentsWithTotal = new
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
    public async Task Should_also_fetch_union_contents_if_field_is_included_in_query()
    {
        var contentRefId = DomainId.NewGuid();
        var contentRef = TestContent.CreateRef(TestSchemas.Ref1Id, contentRefId, "schemaRef1Field", "ref1");

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, contentRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    data {
                      myUnion {
                        iv {
                          ... on Content {
                            id
                          }
                          ... on MyRefSchema1 {
                            data {
                              schemaRef1Field {
                                iv
                              }
                            }
                          }
                          __typename
                        }
                      }
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, contentRef));

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
                                        schemaRef1Field = new
                                        {
                                            iv = "ref1"
                                        }
                                    },
                                    __typename = "MyRefSchema1"
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
    public async Task Should_also_fetch_embedded_assets_if_field_is_included_in_query()
    {
        var assetRefId = DomainId.NewGuid();
        var assetRef = TestAsset.Create(assetRefId);

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, assetId: assetRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
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
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, assetRef));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
                                text = $"assets:{assetRefId}, contents:{DomainId.Empty}",
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
    public async Task Should_also_fetch_referenced_assets_if_field_is_included_in_query()
    {
        var assetRefId = DomainId.NewGuid();
        var assetRef = TestAsset.Create(assetRefId);

        var contentId = DomainId.NewGuid();
        var content = TestContent.Create(contentId, assetId: assetRefId);

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    data {
                      myAssets {
                        iv {
                          id
                        }
                      }
                    }
                  }
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                A<Q>.That.HasIdsWithoutTotal(assetRefId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(0, assetRef));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

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
        var content = TestContent.Create(contentId, data: new ContentData());

        var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
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
                }", contentId);

        A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(),
                A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(1, content));

        var actual = await ExecuteAsync(new ExecutionOptions { Query = query });

        var json = serializer.Serialize(actual);

        Assert.Contains("\"errors\"", json, StringComparison.Ordinal);
    }
}
