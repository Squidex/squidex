// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
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
            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

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
            const string query = @"
                query {
                  queryAssets(filter: ""my-query"", top: 30, skip: 5) {
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    url
                    thumbnailUrl
                    sourceUrl
                    mimeType
                    fileName
                    fileHash
                    fileSize
                    fileVersion
                    isImage
                    pixelWidth
                    pixelHeight
                    tags
                    slug
                  }
                }";

            var asset = CreateAsset(Guid.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query")))
                .Returns(ResultList.CreateFrom(0, asset));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    queryAssets = new dynamic[]
                    {
                        new
                        {
                            id = asset.Id,
                            version = 1,
                            created = asset.Created,
                            createdBy = "subject:user1",
                            lastModified = asset.LastModified,
                            lastModifiedBy = "subject:user2",
                            url = $"assets/{asset.Id}",
                            thumbnailUrl = $"assets/{asset.Id}?width=100",
                            sourceUrl = $"assets/source/{asset.Id}",
                            mimeType = "image/png",
                            fileName = "MyFile.png",
                            fileHash = "ABC123",
                            fileSize = 1024,
                            fileVersion = 123,
                            isImage = true,
                            pixelWidth = 800,
                            pixelHeight = 600,
                            tags = new string[] { "tag1", "tag2" },
                            slug = "myfile.png"
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_multiple_assets_with_total_when_querying_assets_with_total()
        {
            const string query = @"
                query {
                  queryAssetsWithTotal(filter: ""my-query"", top: 30, skip: 5) {
                    total
                    items {
                      id
                      version
                      created
                      createdBy
                      lastModified
                      lastModifiedBy
                      url
                      thumbnailUrl
                      sourceUrl
                      mimeType
                      fileName
                      fileHash
                      fileSize
                      fileVersion
                      isImage
                      pixelWidth
                      pixelHeight
                      tags
                      slug
                    }   
                  }
                }";

            var asset = CreateAsset(Guid.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query")))
                .Returns(ResultList.CreateFrom(10, asset));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    queryAssetsWithTotal = new
                    {
                        total = 10,
                        items = new dynamic[]
                        {
                            new
                            {
                                id = asset.Id,
                                version = 1,
                                created = asset.Created,
                                createdBy = "subject:user1",
                                lastModified = asset.LastModified,
                                lastModifiedBy = "subject:user2",
                                url = $"assets/{asset.Id}",
                                thumbnailUrl = $"assets/{asset.Id}?width=100",
                                sourceUrl = $"assets/source/{asset.Id}",
                                mimeType = "image/png",
                                fileName = "MyFile.png",
                                fileHash = "ABC123",
                                fileSize = 1024,
                                fileVersion = 123,
                                isImage = true,
                                pixelWidth = 800,
                                pixelHeight = 600,
                                tags = new string[] { "tag1", "tag2" },
                                slug = "myfile.png"
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_single_asset_when_finding_asset()
        {
            var assetId = Guid.NewGuid();
            var asset = CreateAsset(assetId);

            var query = @"
                query {
                  findAsset(id: ""<ID>"") {
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    url
                    thumbnailUrl
                    sourceUrl
                    mimeType
                    fileName
                    fileHash
                    fileSize
                    fileVersion
                    isImage
                    pixelWidth
                    pixelHeight
                    tags
                    slug
                  }
                }".Replace("<ID>", assetId.ToString());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), MatchId(assetId)))
                .Returns(ResultList.CreateFrom(1, asset));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findAsset = new
                    {
                        id = asset.Id,
                        version = 1,
                        created = asset.Created,
                        createdBy = "subject:user1",
                        lastModified = asset.LastModified,
                        lastModifiedBy = "subject:user2",
                        url = $"assets/{asset.Id}",
                        thumbnailUrl = $"assets/{asset.Id}?width=100",
                        sourceUrl = $"assets/source/{asset.Id}",
                        mimeType = "image/png",
                        fileName = "MyFile.png",
                        fileHash = "ABC123",
                        fileSize = 1024,
                        fileVersion = 123,
                        isImage = true,
                        pixelWidth = 800,
                        pixelHeight = 600,
                        tags = new string[] { "tag1", "tag2" },
                        slug = "myfile.png"
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_multiple_contents_when_querying_contents()
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
                    data {
                      myString {
                        de
                      }
                      myNumber {
                        iv
                      }
                      myBoolean {
                        iv
                      }
                      myDatetime {
                        iv
                      }
                      myJson {
                        iv
                      }
                      myGeolocation {
                        iv
                      }
                      myTags {
                        iv
                      }
                      myLocalized {
                        de_DE
                      }
                      myArray {
                        iv {
                          nestedNumber
                          nestedBoolean
                        }
                      }
                    }
                  }
                }";

            var content = CreateContent(Guid.NewGuid(), Guid.Empty, Guid.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5")))
                .Returns(ResultList.CreateFrom(0, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

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
                                    iv = content.LastModified
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
                                },
                                myLocalized = new
                                {
                                    de_DE = "de-DE"
                                },
                                myArray = new
                                {
                                    iv = new[]
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
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_multiple_contents_with_total_when_querying_contents_with_total()
        {
            const string query = @"
                query {
                  queryMySchemaContentsWithTotal(top: 30, skip: 5) {
                    total
                    items {
                      id
                      version
                      created
                      createdBy
                      lastModified
                      lastModifiedBy
                      status
                      statusColor
                      url
                      data {
                        myString {
                          de
                        }
                        myNumber {
                          iv
                        }
                        myBoolean {
                          iv
                        }
                        myDatetime {
                          iv
                        }
                        myJson {
                          iv
                        }
                        myGeolocation {
                          iv
                        }
                        myTags {
                          iv
                        }
                        myLocalized {
                          de_DE
                        }
                      }
                    }
                  }
                }";

            var content = CreateContent(Guid.NewGuid(), Guid.Empty, Guid.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5")))
                .Returns(ResultList.CreateFrom(10, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    queryMySchemaContentsWithTotal = new
                    {
                        total = 10,
                        items = new dynamic[]
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
                                        iv = content.LastModified
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
                                    },
                                    myLocalized = new
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
        public async Task Should_return_single_content_with_duplicate_names()
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, Guid.Empty);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    data {
                      myNumber {
                        iv
                      }
                      myNumber2 {
                        iv
                      }
                      myArray {
                        iv {
                          nestedNumber
                          nestedNumber2
                        }
                      }
                    }
                  }
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = new
                    {
                        data = new
                        {
                            myNumber = new
                            {
                                iv = 1
                            },
                            myNumber2 = new
                            {
                                iv = 2
                            },
                            myArray = new
                            {
                                iv = new[]
                                {
                                    new
                                    {
                                        nestedNumber = 10,
                                        nestedNumber2 = 11,
                                    },
                                    new
                                    {
                                        nestedNumber = 20,
                                        nestedNumber2 = 21,
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
        public async Task Should_return_single_content_when_finding_content()
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, Guid.Empty);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    status
                    statusColor
                    url
                    data {
                      myString {
                        de
                      }
                      myNumber {
                        iv
                      }
                      myBoolean {
                        iv
                      }
                      myDatetime {
                        iv
                      }
                      myJson {
                        iv
                      }
                      myGeolocation {
                        iv
                      }
                      myTags {
                        iv
                      }
                      myLocalized {
                        de_DE
                      }
                    }
                  }
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = new
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
                                iv = content.LastModified
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
                            },
                            myLocalized = new
                            {
                                de_DE = "de-DE"
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_also_fetch_referenced_contents_when_field_is_included_in_query()
        {
            var contentRefId = Guid.NewGuid();
            var contentRef = CreateContent(contentRefId, Guid.Empty, Guid.Empty);

            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, contentRefId, Guid.Empty);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    id
                    data {
                      myReferences {
                        iv {
                          id
                        }
                      }
                    }
                  }
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), A<Q>.Ignored))
                .Returns(ResultList.CreateFrom(0, contentRef));

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

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
                                        id = contentRefId
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
            var assetRefId = Guid.NewGuid();
            var assetRef = CreateAsset(assetRefId);

            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, assetRefId);

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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), A<Q>.Ignored))
                .Returns(ResultList.CreateFrom(0, assetRef));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

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
            var assetId1 = Guid.NewGuid();
            var assetId2 = Guid.NewGuid();
            var asset1 = CreateAsset(assetId1);
            var asset2 = CreateAsset(assetId2);

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

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), MatchId(assetId1)))
                .Returns(ResultList.CreateFrom(0, asset1));

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), MatchId(assetId2)))
                .Returns(ResultList.CreateFrom(0, asset2));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query1 }, new GraphQLQuery { Query = query2 });

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
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, Guid.Empty, new NamedContentData());

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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var json = serializer.Serialize(result);

            Assert.Contains("\"data\":null", json);
        }

        [Fact]
        public async Task Should_return_draft_content_when_querying_dataDraft()
        {
            var dataDraft = new NamedContentData()
                .AddField("my-string",
                    new ContentFieldData()
                        .AddValue("de", "draft value"))
                .AddField("my-number",
                    new ContentFieldData()
                        .AddValue("iv", 42));

            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, Guid.Empty, null, dataDraft);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    dataDraft {
                      myString {
                        de
                      }
                      myNumber {
                        iv
                      }
                    }
                  }
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = new
                    {
                        dataDraft = new
                        {
                            myString = new
                            {
                                de = "draft value"
                            },
                            myNumber = new
                            {
                                iv = 42
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_null_when_querying_dataDraft_and_no_draft_content_is_available()
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, Guid.Empty, null);

            var query = @"
                query {
                  findMySchemaContent(id: ""<ID>"") {
                    dataDraft {
                      myString {
                        de
                      }
                    }
                  }
                }".Replace("<ID>", contentId.ToString());

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(context, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = new
                    {
                        dataDraft = (object)null
                    }
                }
            };

            AssertResult(expected, result);
        }

        private static Q MatchId(Guid contentId)
        {
            return A<Q>.That.Matches(x => x.Ids.Count == 1 && x.Ids[0] == contentId);
        }

        private Context MatchsAssetContext()
        {
            return A<Context>.That.Matches(x => x.App == app && x.User == user);
        }

        private Context MatchsContentContext()
        {
            return A<Context>.That.Matches(x => x.App == app && x.User == user);
        }
    }
}
