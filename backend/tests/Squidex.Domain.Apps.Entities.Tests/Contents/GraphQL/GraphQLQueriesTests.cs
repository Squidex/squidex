// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
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
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Should_return_empty_object_for_empty_query(string query)
        {
            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_multiple_assets_when_querying_assets()
        {
            var query = @"
                query {
                  queryAssets(filter: ""my-query"", top: 30, skip: 5) {
                    <FIELDS>
                  }
                }".Replace("<FIELDS>", TestAsset.AllFields);

            var asset = TestAsset.Create(appId, DomainId.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasOData("?$top=30&$skip=5&$filter=my-query")))
                .Returns(ResultList.CreateFrom(0, asset));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_return_multiple_assets_with_total_when_querying_assets_with_total()
        {
            var query = @"
                query {
                  queryAssetsWithTotal(filter: ""my-query"", top: 30, skip: 5) {
                    total
                    items {
                      <FIELDS>
                    }
                  }
                }".Replace("<FIELDS>", TestAsset.AllFields);

            var asset = TestAsset.Create(appId, DomainId.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.HasOData("?$top=30&$skip=5&$filter=my-query")))
                .Returns(ResultList.CreateFrom(10, asset));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_return_null_single_asset()
        {
            var assetId = DomainId.NewGuid();

            var query = @"
                query {
                  findAsset(id: ""<ID>"") {
                    id
                  }
                }".Replace("<ID>", assetId.ToString());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, MatchIdQuery(assetId)))
                .Returns(ResultList.CreateFrom<IEnrichedAssetEntity>(1));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_return_single_asset_when_finding_asset()
        {
            var assetId = DomainId.NewGuid();
            var asset = TestAsset.Create(appId, assetId);

            var query = @"
                query {
                  findAsset(id: ""<ID>"") {
                    <FIELDS>
                  }
                }".Replace("<ID>", assetId.ToString()).Replace("<FIELDS>", TestAsset.AllFields);

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, MatchIdQuery(assetId)))
                .Returns(ResultList.CreateFrom(1, asset));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_return_multiple_flat_contents_when_querying_contents()
        {
            const string query = @"
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
                      myJsonValue: myJson(path: ""value"")
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
                }";

            var content = TestContent.Create(appId, schemaId, DomainId.NewGuid(), DomainId.Empty, DomainId.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.HasOData("?$top=30&$skip=5")))
                .Returns(ResultList.CreateFrom(0, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
                            lastModifiedBy = "subject:user2",
                            status = "DRAFT",
                            statusColor = "red",
                            url = $"contents/my-schema/{content.Id}",
                            flatData = new
                            {
                                myString = "value",
                                myNumber = 1,
                                myBoolean = true,
                                myDatetime = content.LastModified,
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
                                        nestedNumber = 10,
                                        nestedBoolean = true
                                    },
                                    new
                                    {
                                        nestedNumber = 20,
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
        public async Task Should_return_multiple_contents_when_querying_contents()
        {
            var query = @"
                query {
                  queryMySchemaContents(top: 30, skip: 5) {
                    <FIELDS>
                  }
                }".Replace("<FIELDS>", TestContent.AllFields);

            var content = TestContent.Create(appId, schemaId, DomainId.NewGuid(), DomainId.Empty, DomainId.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.HasOData("?$top=30&$skip=5")))
                .Returns(ResultList.CreateFrom(0, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_return_multiple_contents_with_total_when_querying_contents_with_total()
        {
            var query = @"
                query {
                  queryMySchemaContentsWithTotal(top: 30, skip: 5) {
                    total
                    items {
                      <FIELDS>
                    }
                  }
                }".Replace("<FIELDS>", TestContent.AllFields);

            var content = TestContent.Create(appId, schemaId, DomainId.NewGuid(), DomainId.Empty, DomainId.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.HasOData("?$top=30&$skip=5")))
                .Returns(ResultList.CreateFrom(10, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_return_null_single_content()
        {
            var contentId = DomainId.NewGuid();

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    id
                  }
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom<IEnrichedContentEntity>(1));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_return_single_content_when_finding_content()
        {
            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, DomainId.Empty, DomainId.Empty);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    <FIELDS>
                  }
                }".Replace("<FIELDS>", TestContent.AllFields).Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_also_fetch_referenced_contents_when_field_is_included_in_query()
        {
            var contentRefId = DomainId.NewGuid();
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
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
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<IReadOnlyList<DomainId>>._))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_also_fetch_referencing_contents_when_field_is_included_in_query()
        {
            var contentRefId = DomainId.NewGuid();
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

            var query = @"
                query {
                  findMyRefSchema1Content(id: ""<ID>"") {
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
                }".Replace("<ID>", contentRefId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<IReadOnlyList<DomainId>>._))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.HasOData("?$top=30&$skip=5", contentRefId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_also_fetch_referencing_contents_with_total_when_field_is_included_in_query()
        {
            var contentRefId = DomainId.NewGuid();
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

            var query = @"
                query {
                  findMyRefSchema1Content(id: ""<ID>"") {
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
                }".Replace("<ID>", contentRefId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<IReadOnlyList<DomainId>>._))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.HasOData("?$top=30&$skip=5", contentRefId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_also_fetch_union_contents_when_field_is_included_in_query()
        {
            var contentRefId = DomainId.NewGuid();
            var contentRef = TestContent.CreateRef(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, contentRefId, DomainId.Empty);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
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
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<IReadOnlyList<DomainId>>._))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_also_fetch_referenced_assets_when_field_is_included_in_query()
        {
            var assetRefId = DomainId.NewGuid();
            var assetRef = TestAsset.Create(appId, assetRefId);

            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, DomainId.Empty, assetRefId);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    id
                    data {
                      myAssets {
                        iv {
                          id
                        }
                      }
                    }
                  }
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>._))
                .Returns(ResultList.CreateFrom(0, assetRef));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
        public async Task Should_make_multiple_queries()
        {
            var assetId1 = DomainId.NewGuid();
            var assetId2 = DomainId.NewGuid();
            var asset1 = TestAsset.Create(appId, assetId1);
            var asset2 = TestAsset.Create(appId, assetId2);

            var query1 = @"
                query {
                  findAsset(id: ""<ID>"") {
                    id
                  }
                }".Replace("<ID>", assetId1.ToString());
            var query2 = @"
                query {
                  findAsset(id: ""<ID>"") {
                    id
                  }
                }".Replace("<ID>", assetId2.ToString());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, MatchIdQuery(assetId1)))
                .Returns(ResultList.CreateFrom(0, asset1));

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, MatchIdQuery(assetId2)))
                .Returns(ResultList.CreateFrom(0, asset2));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query1 }, new GraphQLQuery { Query = query2 });

            var expected = new object[]
            {
                new
                {
                    data = new
                    {
                        findAsset = new
                        {
                            id = asset1.Id
                        }
                    }
                },
                new
                {
                    data = new
                    {
                        findAsset = new
                        {
                            id = asset2.Id
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_not_return_data_when_field_not_part_of_content()
        {
            var contentId = DomainId.NewGuid();
            var content = TestContent.Create(appId, schemaId, contentId, DomainId.Empty, DomainId.Empty, new NamedContentData());

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
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
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var json = serializer.Serialize(result);

            Assert.Contains("\"data\":null", json);
        }

        private static IReadOnlyList<DomainId> MatchId(DomainId contentId)
        {
            return A<IReadOnlyList<DomainId>>.That.Matches(x => x.Count == 1 && x[0] == contentId);
        }

        private static Q MatchIdQuery(DomainId contentId)
        {
            return A<Q>.That.Matches(x => x.Ids.Count == 1 && x.Ids[0] == contentId);
        }

        private Context MatchsAssetContext()
        {
            return A<Context>.That.Matches(x => x.App == app && x.User == requestContext.User);
        }

        private Context MatchsContentContext()
        {
            return A<Context>.That.Matches(x => x.App == app && x.User == requestContext.User);
        }
    }
}
