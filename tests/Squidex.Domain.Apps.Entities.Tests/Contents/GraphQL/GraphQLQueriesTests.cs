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
                  queryAssets(search: ""my-query"", take: 30, skip: 5) {
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
                    fileSize
                    fileVersion
                    isImage
                    pixelWidth
                    pixelHeight
                  }
                }";

            var asset = CreateAsset(Guid.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), A<Q>.That.Matches(x => x.ODataQuery == "?$take=30&$skip=5&$search=my-query")))
                .Returns(ResultList.Create(0, asset));

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
                            fileSize = 1024,
                            fileVersion = 123,
                            isImage = true,
                            pixelWidth = 800,
                            pixelHeight = 600
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
                  queryAssetsWithTotal(search: ""my-query"", take: 30, skip: 5) {
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
                      fileSize
                      fileVersion
                      isImage
                      pixelWidth
                      pixelHeight
                    }   
                  }
                }";

            var asset = CreateAsset(Guid.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), A<Q>.That.Matches(x => x.ODataQuery == "?$take=30&$skip=5&$search=my-query")))
                .Returns(ResultList.Create(10, asset));

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
                                fileSize = 1024,
                                fileVersion = 123,
                                isImage = true,
                                pixelWidth = 800,
                                pixelHeight = 600
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

            var query = $@"
                query {{
                  findAsset(id: ""{assetId}"") {{
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
                    fileSize
                    fileVersion
                    isImage
                    pixelWidth
                    pixelHeight
                  }}
                }}";

            A.CallTo(() => assetQuery.FindAssetAsync(MatchsAssetContext(), assetId))
                .Returns(asset);

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
                        fileSize = 1024,
                        fileVersion = 123,
                        isImage = true,
                        pixelWidth = 800,
                        pixelHeight = 600
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
                .Returns(ResultList.Create(0, content));

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
                                            nestedNumber = 1,
                                            nestedBoolean = true
                                        },
                                        new
                                        {
                                            nestedNumber = 2,
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
                .Returns(ResultList.Create(10, content));

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
        public async Task Should_return_single_content_when_finding_content()
        {
            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, Guid.Empty, Guid.Empty);

            var query = $@"
                query {{
                  findMySchemaContent(id: ""{contentId}"") {{
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    status
                    url
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
                      myJson {{
                        iv
                      }}
                      myGeolocation {{
                        iv
                      }}
                      myTags {{
                        iv
                      }}
                      myLocalized {{
                        de_DE
                      }}
                    }}
                  }}
                }}";

            A.CallTo(() => contentQuery.FindContentAsync(MatchsContentContext(), schemaId.ToString(), contentId, EtagVersion.Any))
                .Returns(content);

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

            var query = $@"
                query {{
                  findMySchemaContent(id: ""{contentId}"") {{
                    id
                    data {{
                      myReferences {{
                        iv {{
                          id
                        }}
                      }}
                    }}
                  }}
                }}";

            A.CallTo(() => contentQuery.FindContentAsync(MatchsContentContext(), schemaId.ToString(), contentId, EtagVersion.Any))
                .Returns(content);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.ToString(), A<Q>.Ignored))
                .Returns(ResultList.Create(0, contentRef));

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

            var query = $@"
                query {{
                  findMySchemaContent(id: ""{contentId}"") {{
                    id
                    data {{
                      myAssets {{
                        iv {{
                          id
                        }}
                      }}
                    }}
                  }}
                }}";

            A.CallTo(() => contentQuery.FindContentAsync(MatchsContentContext(), schemaId.ToString(), contentId, EtagVersion.Any))
                .Returns(content);

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), A<Q>.Ignored))
                .Returns(ResultList.Create(0, assetRef));

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

            var query1 = $@"
                query {{
                  findAsset(id: ""{assetId1}"") {{
                    id
                  }}
                }}";
            var query2 = $@"
                query {{
                  findAsset(id: ""{assetId2}"") {{
                    id
                  }}
                }}";

            A.CallTo(() => assetQuery.FindAssetAsync(MatchsAssetContext(), assetId1))
                .Returns(asset1);
            A.CallTo(() => assetQuery.FindAssetAsync(MatchsAssetContext(), assetId2))
                .Returns(asset2);

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

            var query = $@"
                query {{
                  findMySchemaContent(id: ""{contentId}"") {{
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    url
                    data {{
                      myInvalid {{
                        iv
                      }}
                    }}
                  }}
                }}";

            A.CallTo(() => contentQuery.FindContentAsync(MatchsContentContext(), schemaId.ToString(), contentId, EtagVersion.Any))
                .Returns(content);

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

            var query = $@"
                query {{
                  findMySchemaContent(id: ""{contentId}"") {{
                    dataDraft {{
                      myString {{
                        de
                      }}
                      myNumber {{
                        iv
                      }}
                    }}
                  }}
                }}";

            A.CallTo(() => contentQuery.FindContentAsync(MatchsContentContext(), schemaId.ToString(), contentId, EtagVersion.Any))
                .Returns(content);

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

            var query = $@"
                query {{
                  findMySchemaContent(id: ""{contentId}"") {{
                    dataDraft {{
                      myString {{
                        de
                      }}
                    }}
                  }}
                }}";

            A.CallTo(() => contentQuery.FindContentAsync(MatchsContentContext(), schemaId.ToString(), contentId, EtagVersion.Any))
                .Returns(content);

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

        private QueryContext MatchsAssetContext()
        {
            return A<QueryContext>.That.Matches(x => x.App == app && x.User == user && !x.Archived);
        }

        private QueryContext MatchsContentContext()
        {
            return A<QueryContext>.That.Matches(x => x.App == app && x.User == user && !x.Archived);
        }
    }
}
