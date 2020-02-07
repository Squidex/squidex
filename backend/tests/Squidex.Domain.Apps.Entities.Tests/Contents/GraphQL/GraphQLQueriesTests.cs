// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLQueriesTests : GraphQLTestBase
    {
        [Fact]
        public async Task Should_introspect()
        {
            const string query = @"
                query IntrospectionQuery {
                  __schema {
                    queryType { name }
                    mutationType { name }
                    subscriptionType { name }
                    types {
                      ...FullType
                    }
                    directives {
                      name
                      description
                      args {
                        ...InputValue
                      }
                      onOperation
                      onFragment
                      onField
                    }
                  }
                }

                fragment FullType on __Type {
                  kind
                  name
                  description
                  fields(includeDeprecated: true) {
                    name
                    description
                    args {
                      ...InputValue
                    }
                    type {
                      ...TypeRef
                    }
                    isDeprecated
                    deprecationReason
                  }
                  inputFields {
                    ...InputValue
                  }
                  interfaces {
                    ...TypeRef
                  }
                  enumValues(includeDeprecated: true) {
                    name
                    description
                    isDeprecated
                    deprecationReason
                  }
                  possibleTypes {
                    ...TypeRef
                  }
                }

                fragment InputValue on __InputValue {
                  name
                  description
                  type { ...TypeRef }
                  defaultValue
                }

                fragment TypeRef on __Type {
                  kind
                  name
                  ofType {
                    kind
                    name
                    ofType {
                      kind
                      name
                      ofType {
                        kind
                        name
                      }
                    }
                  }
                }";

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query, OperationName = "IntrospectionQuery" });

            var json = serializer.Serialize(result.Response, true);

            Assert.NotEmpty(json);
        }

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
                    isProtected
                    pixelWidth
                    pixelHeight
                    type
                    metadataText
                    metadataPixelWidth: metadata(path: ""pixelWidth"")
                    metadataUnknown: metadata(path: ""unknown"")
                    metadata
                    tags
                    slug
                  }
                }";

            var asset = CreateAsset(Guid.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query")))
                .Returns(ResultList.CreateFrom(0, asset));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
                            isProtected = false,
                            pixelWidth = 800,
                            pixelHeight = 600,
                            type = "IMAGE",
                            metadataText = "metadata-text",
                            metadataPixelWidth = 800,
                            metadataUnknown = (string?)null,
                            metadata = new
                            {
                                pixelWidth = 800,
                                pixelHeight = 600
                            },
                            tags = new[]
                            {
                                "tag1",
                                "tag2"
                            },
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
                      isProtected
                      pixelWidth
                      pixelHeight
                      type
                      metadataText
                      metadataPixelWidth: metadata(path: ""pixelWidth"")
                      metadataUnknown: metadata(path: ""unknown"")
                      metadata
                      tags
                      slug
                    }
                  }
                }";

            var asset = CreateAsset(Guid.NewGuid());

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5&$filter=my-query")))
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
                                isProtected = false,
                                pixelWidth = 800,
                                pixelHeight = 600,
                                type = "IMAGE",
                                metadataText = "metadata-text",
                                metadataPixelWidth = 800,
                                metadataUnknown = (string?)null,
                                metadata = new
                                {
                                    pixelWidth = 800,
                                    pixelHeight = 600
                                },
                                tags = new[]
                                {
                                    "tag1",
                                    "tag2"
                                },
                                slug = "myfile.png"
                            }
                        }
                    }
                }
            };

            AssertResult(expected, result);
        }

        [Fact]
        public async Task Should_return_null_single_asset()
        {
            var assetId = Guid.NewGuid();

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

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, MatchIdQuery(assetId)))
                .Returns(ResultList.CreateFrom(1, asset));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
                        tags = new[] { "tag1", "tag2" },
                        slug = "myfile.png"
                    }
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

            var content = CreateContent(Guid.NewGuid(), Guid.Empty, Guid.Empty);

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5")))
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5")))
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), schemaId.Id.ToString(), A<Q>.That.Matches(x => x.ODataQuery == "?$top=30&$skip=5")))
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

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
                                        nestedNumber2 = 11
                                    },
                                    new
                                    {
                                        nestedNumber = 20,
                                        nestedNumber2 = 21
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
        public async Task Should_return_null_single_content()
        {
            var contentId = Guid.NewGuid();

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
            var contentRef = CreateRefContent(schemaRefId1, contentRefId, "ref1-field", "ref1");

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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<IReadOnlyList<Guid>>.Ignored))
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
        public async Task Should_also_fetch_union_contents_when_field_is_included_in_query()
        {
            var contentRefId = Guid.NewGuid();
            var contentRef = CreateRefContent(schemaRefId1, contentRefId, "ref1-field", "ref1");

            var contentId = Guid.NewGuid();
            var content = CreateContent(contentId, contentRefId, Guid.Empty);

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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), A<IReadOnlyList<Guid>>.Ignored))
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            A.CallTo(() => assetQuery.QueryAsync(MatchsAssetContext(), null, A<Q>.Ignored))
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

            A.CallTo(() => contentQuery.QueryAsync(MatchsContentContext(), MatchId(contentId)))
                .Returns(ResultList.CreateFrom(1, content));

            var result = await sut.QueryAsync(requestContext, new GraphQLQuery { Query = query });

            var json = serializer.Serialize(result);

            Assert.Contains("\"data\":null", json);
        }

        private static IReadOnlyList<Guid> MatchId(Guid contentId)
        {
            return A<IReadOnlyList<Guid>>.That.Matches(x => x.Count == 1 && x[0] == contentId);
        }

        private static Q MatchIdQuery(Guid contentId)
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
