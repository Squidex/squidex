﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using GraphQL;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLQueriesTests : GraphQLTestBase
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Should_return_error_empty_query(string query)
        {
            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query" && x.NoTotal == true), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(0, asset));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query" && x.NoTotal == false), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(10, asset));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_null_single_asset_if_not_found()
        {
            var assetId = DomainId.NewGuid();

            var query = CreateQuery(@"
                query {
                  findAsset(id: '<ID>') {
                    id
                  }
                }", assetId);

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasIdsWithoutTotal(assetId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom<IEnrichedAssetEntity>(1));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    findAsset = (object?)null
                }
            };

            AssertResult(expected, result);
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

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasIdsWithoutTotal(assetId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, asset));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    findAsset = TestAsset.Response(asset)
                }
            };

            AssertResult(expected, result);
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
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.NoTotal == true), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(0, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.NoTotal == true), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(0, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.NoTotal == false), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(10, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_null_single_content_if_not_found()
        {
            var contentId = DomainId.NewGuid();

            var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                  }
                }", contentId);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom<IEnrichedContentEntity>(1));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = (object?)null
                }
            };

            AssertResult(expected, result);
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);
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

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = TestContent.Response(content)
                }
            };

            AssertResult(expected, result);
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.Reference == contentRefId && x.NoTotal == true), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.Reference == contentRefId && x.NoTotal == false), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    findMyRefSchema1Content = new
                    {
                        id = contentRefId,
                        referencingMySchemaContentsWithTotal = new
                        {
                            total = 1,
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

            AssertResult(expected, result);
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, content));

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasIdsWithoutTotal(assetRefId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(0, assetRef));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

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

            AssertResult(expected, result);
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var json = serializer.Serialize(result);

            Assert.Contains("\"errors\"", json);
        }

        private static string CreateQuery(string query, DomainId id = default)
        {
            return query
                .Replace("'", "\"")
                .Replace("<ID>", id.ToString())
                .Replace("<FIELDS_ASSET>", TestAsset.AllFields)
                .Replace("<FIELDS_CONTENT>", TestContent.AllFields)
                .Replace("<FIELDS_CONTENT_FLAT>", TestContent.AllFlatFields);
        }

        private Context MatchsAssetContext()
        {
            return A<Context>.That.Matches(x =>
                x.App == TestApp.Default &&
                x.ShouldSkipCleanup() &&
                x.ShouldSkipContentEnrichment() &&
                x.User == requestContext.User);
        }

        private Context MatchsContentContext()
        {
            return A<Context>.That.Matches(x =>
                x.App == TestApp.Default &&
                x.ShouldSkipCleanup() &&
                x.ShouldSkipContentEnrichment() &&
                x.User == requestContext.User);
        }
    }
}
