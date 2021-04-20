// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

            var asset = TestAsset.Create(appId, DomainId.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query" && x.NoTotal == true)))
                .Returns(ResultList.CreateFrom(0, asset));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    queryAssets = new dynamic[]
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

            var asset = TestAsset.Create(appId, DomainId.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null,
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query" && x.NoTotal == false)))
                .Returns(ResultList.CreateFrom(10, asset));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    queryAssetsWithTotal = new
                    {
                        total = 10,
                        items = new dynamic[]
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

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasIdsWithoutTotal(assetId)))
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
            var asset = TestAsset.Create(appId, assetId);

            var query = CreateQuery(@"
                query {
                  findAsset(id: '<ID>') {
                    <FIELDS_ASSET>
                  }
                }", assetId);

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasIdsWithoutTotal(assetId)))
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
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    status
                    statusColor
                    url
                    flatData {
                      myString
                      myNumber
                      myBoolean
                      myDatetime
                      myJsonValue: myJson(path: 'value')
                      myJson
                      myGeolocation
                      myTags
                      myLocalized
                      myArray {
                        nestedNumber
                        nestedBoolean
                      }
                    }
                  }
                }");

            var content = TestContent.Create(appId, schemaId, DomainId.NewGuid(), DomainId.Empty, DomainId.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(),
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.NoTotal == true)))
                .Returns(ResultList.CreateFrom(0, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    queryMySchemaContents = new dynamic[]
                    {
                        new
                        {
                            id = content.Id,
                            version = 1,
                            created = content.Created,
                            createdBy = "subject:user1",
                            lastModified = content.LastModified,
                            lastModifiedBy = "client:client1",
                            status = "DRAFT",
                            statusColor = "red",
                            url = $"contents/my-schema/{content.Id}",
                            flatData = new
                            {
                                myString = "value",
                                myNumber = 1.0,
                                myBoolean = true,
                                myDatetime = content.LastModified.ToString(),
                                myJsonValue = 1,
                                myJson = new
                                {
                                    value = 1
                                },
                                myGeolocation = new
                                {
                                    latitude = 10,
                                    longitude = 20
                                },
                                myTags = new[]
                                {
                                    "tag1",
                                    "tag2"
                                },
                                myLocalized = "de-DE",
                                myArray = new[]
                                {
                                    new
                                    {
                                        nestedNumber = 10.0,
                                        nestedBoolean = true
                                    },
                                    new
                                    {
                                        nestedNumber = 20.0,
                                        nestedBoolean = false
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
        public async Task Should_return_multiple_contents_if_querying_contents()
        {
            var query = CreateQuery(@"
                query {
                  queryMySchemaContents(top: 30, skip: 5) {
                    <FIELDS_CONTENT>
                  }
                }");

            var content = TestContent.Create(appId, schemaId, DomainId.NewGuid(), DomainId.Empty, DomainId.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(),
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.NoTotal == true)))
                .Returns(ResultList.CreateFrom(0, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    queryMySchemaContents = new dynamic[]
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

            var content = TestContent.Create(appId, schemaId, DomainId.NewGuid(), DomainId.Empty, DomainId.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(),
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.NoTotal == false)))
                .Returns(ResultList.CreateFrom(10, content));

            var result = await ExecuteAsync(new ExecutionOptions { Query = query });

            var expected = new
            {
                data = new
                {
                    queryMySchemaContentsWithTotal = new
                    {
                        total = 10,
                        items = new dynamic[]
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId)))
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
            var content = TestContent.Create(appId, schemaId, contentId, DomainId.Empty, DomainId.Empty);

            var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    <FIELDS_CONTENT>
                  }
                }", contentId);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId)))
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
            var content = TestContent.Create(appId, schemaId, contentId, DomainId.Empty, DomainId.Empty);

            var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>', version: 3) {
                    <FIELDS_CONTENT>
                  }
                }", contentId);

            A.CallTo(() => contentQuery.FindAsync(MatchsContentContext(), schemaId.Id.ToString(), contentId, 3))
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
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

            var query = CreateQuery(@"
                query {
                  findMySchemaContent(id: '<ID>') {
                    id
                    data {
                      myReferences {
                        iv {
                          id
                          data {
                            ref1Field {
                              iv
                            }
                          }
                        }
                      }
                    }
                  }
                }", contentId);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId)))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId)))
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
                                            ref1Field = new
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
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

            var query = CreateQuery(@"
                query {
                  findMyRefSchema1Content(id: '<ID>') {
                    id
                    referencingMySchemaContents(top: 30, skip: 5) {
                      id
                      data {
                        myString {
                          de
                        }
                      }
                    }
                  }
                }", contentRefId);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId)))
                .Returns(ResultList.CreateFrom(1, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.Reference == contentRefId && x.NoTotal == true)))
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
                                    myString = new
                                    {
                                        de = "value"
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
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

            var query = CreateQuery(@"
                query {
                  findMyRefSchema1Content(id: '<ID>') {
                    id
                    referencingMySchemaContentsWithTotal(top: 30, skip: 5) {
                      total
                      items {
                        id
                        data {
                          myString {
                            de
                          }
                        }
                      }
                    }
                  }
                }", contentRefId);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId)))
                .Returns(ResultList.CreateFrom(1, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), content.SchemaId.Id.ToString(),
                    A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5" && x.Reference == contentRefId && x.NoTotal == false)))
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
                                        myString = new
                                        {
                                            de = "value"
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
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

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
                              ref1Field {
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentRefId)))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId)))
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
                                            ref1Field = new
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
            var assetRef = TestAsset.Create(appId, assetRefId);

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, DomainId.Empty, assetRefId);

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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasIdsWithoutTotal(assetRefId)))
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
            var content = TestContent.Create(appId, schemaId, contentId, DomainId.Empty, DomainId.Empty, new ContentData());

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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<Q>.That.HasIdsWithoutTotal(contentId)))
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
                .Replace("<FIELDS_CONTENT>", TestContent.AllFields);
        }

        private Context MatchsAssetContext()
        {
            return A<Context>.That.Matches(x =>
                x.App == app &&
                x.ShouldSkipCleanup() &&
                x.ShouldSkipContentEnrichment() &&
                x.User == requestContext.User);
        }

        private Context MatchsContentContext()
        {
            return A<Context>.That.Matches(x =>
                x.App == app &&
                x.ShouldSkipCleanup() &&
                x.ShouldSkipContentEnrichment() &&
                x.User == requestContext.User);
        }
    }
}
