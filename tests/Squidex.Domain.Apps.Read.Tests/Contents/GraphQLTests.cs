// ==========================================================================
//  GraphQLTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Contents.TestData;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Xunit;
using NodaTime.Extensions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Infrastructure;

// ReSharper disable SimilarAnonymousTypeNearby

namespace Squidex.Domain.Apps.Read.Contents
{
    public class GraphQLTests
    {
        private static readonly Guid schemaId = Guid.NewGuid();
        private static readonly Guid appId = Guid.NewGuid();

        private readonly Schema schema =
            Schema.Create("my-schema", new SchemaProperties())
                .AddOrUpdateField(new JsonField(1, "my-json", Partitioning.Invariant,
                    new JsonFieldProperties()))
                .AddOrUpdateField(new StringField(2, "my-string", Partitioning.Language,
                    new StringFieldProperties()))
                .AddOrUpdateField(new NumberField(3, "my-number", Partitioning.Invariant,
                    new NumberFieldProperties()))
                .AddOrUpdateField(new AssetsField(4, "my-assets", Partitioning.Invariant,
                    new AssetsFieldProperties()))
                .AddOrUpdateField(new BooleanField(5, "my-boolean", Partitioning.Invariant,
                    new BooleanFieldProperties()))
                .AddOrUpdateField(new DateTimeField(6, "my-datetime", Partitioning.Invariant,
                    new DateTimeFieldProperties()))
                .AddOrUpdateField(new ReferencesField(7, "my-references", Partitioning.Invariant,
                    new ReferencesFieldProperties { SchemaId = schemaId }))
                .AddOrUpdateField(new ReferencesField(9, "my-invalid", Partitioning.Invariant,
                    new ReferencesFieldProperties { SchemaId = Guid.NewGuid() }))
                .AddOrUpdateField(new GeolocationField(9, "my-geolocation", Partitioning.Invariant,
                    new GeolocationFieldProperties()));

        private readonly ISchemaRepository schemaRepository = A.Fake<ISchemaRepository>();
        private readonly ISchemaEntity schemaEntity = A.Fake<ISchemaEntity>();
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        private readonly IAppEntity appEntity = A.Dummy<IAppEntity>();
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IGraphQLService sut;

        public GraphQLTests()
        {
            A.CallTo(() => appEntity.Id).Returns(appId);
            A.CallTo(() => appEntity.PartitionResolver).Returns(x => InvariantPartitioning.Instance);

            A.CallTo(() => schemaEntity.Id).Returns(schemaId);
            A.CallTo(() => schemaEntity.Name).Returns(schema.Name);
            A.CallTo(() => schemaEntity.Schema).Returns(schema);
            A.CallTo(() => schemaEntity.IsPublished).Returns(true);

            var schemas = new List<ISchemaEntity> { schemaEntity };

            A.CallTo(() => schemaRepository.QueryAllAsync(appId)).Returns(Task.FromResult<IReadOnlyList<ISchemaEntity>>(schemas));

            sut = new CachingGraphQLService(cache, schemaRepository, assetRepository, contentRepository, new FakeUrlGenerator());
        }

        [Fact]
        public async Task Should_return_multiple_assets_when_querying_assets()
        {
            const string query = @"
                query {
                  queryAssets(search: ""my-query"", top: 30, skip: 5) {
                    id
                    version
                    created
                    createdBy
                    lastModified
                    lastModifiedBy
                    url
                    thumbnailUrl
                    mimeType
                    fileName
                    fileSize
                    fileVersion
                    isImage
                    pixelWidth
                    pixelHeight
                  }
                }";

            var assetEntity = CreateAsset(Guid.NewGuid());

            var assets = new List<IAssetEntity> { assetEntity };

            A.CallTo(() => assetRepository.QueryAsync(appEntity.Id, null, null, "my-query", 30, 5))
                .Returns(Task.FromResult<IReadOnlyList<IAssetEntity>>(assets));

            var result = await sut.QueryAsync(appEntity, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    queryAssets = new dynamic[]
                    {
                        new
                        {
                            id = assetEntity.Id,
                            version = 1,
                            created = assetEntity.Created.ToDateTimeUtc(),
                            createdBy = "subject:user1",
                            lastModified = assetEntity.LastModified.ToDateTimeUtc(),
                            lastModifiedBy = "subject:user2",
                            url = $"assets/{assetEntity.Id}",
                            thumbnailUrl = $"assets/{assetEntity.Id}?width=100",
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

            AssertJson(expected, new { data = result.Data });
        }

        [Fact]
        public async Task Should_return_single_asset_when_finding_asset()
        {
            var assetId = Guid.NewGuid();
            var assetEntity = CreateAsset(Guid.NewGuid());

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
                    mimeType
                    fileName
                    fileSize
                    fileVersion
                    isImage
                    pixelWidth
                    pixelHeight
                  }}
                }}";

            A.CallTo(() => assetRepository.FindAssetAsync(assetId))
                .Returns(Task.FromResult(assetEntity));

            var result = await sut.QueryAsync(appEntity, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findAsset = new
                    {
                        id = assetEntity.Id,
                        version = 1,
                        created = assetEntity.Created.ToDateTimeUtc(),
                        createdBy = "subject:user1",
                        lastModified = assetEntity.LastModified.ToDateTimeUtc(),
                        lastModifiedBy = "subject:user2",
                        url = $"assets/{assetEntity.Id}",
                        thumbnailUrl = $"assets/{assetEntity.Id}?width=100",
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

            AssertJson(expected, new { data = result.Data });
        }

        [Fact]
        public async Task Should_return_multiple_contens_when_querying_contents()
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
                    url
                    data {
                      myString {
                        iv
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
                    }
                  }
                }";

            var contentEntity = CreateContent(Guid.NewGuid(), Guid.Empty, Guid.Empty);

            var contents = new List<IContentEntity> { contentEntity };

            A.CallTo(() => contentRepository.QueryAsync(appEntity, schemaId, false, null, "?$top=30&$skip=5"))
                .Returns(Task.FromResult<IReadOnlyList<IContentEntity>>(contents));

            var result = await sut.QueryAsync(appEntity, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    queryMySchemaContents = new dynamic[]
                    {
                        new
                        {
                            id = contentEntity.Id,
                            version = 1,
                            created = contentEntity.Created.ToDateTimeUtc(),
                            createdBy = "subject:user1",
                            lastModified = contentEntity.LastModified.ToDateTimeUtc(),
                            lastModifiedBy = "subject:user2",
                            url = $"contents/my-schema/{contentEntity.Id}",
                            data = new
                            {
                                myString = new
                                {
                                    iv = "value"
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
                                    iv = contentEntity.LastModified.ToDateTimeUtc()
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
                                }
                            }
                        }
                    }
                }
            };

            AssertJson(expected, new { data = result.Data });
        }

        [Fact]
        public async Task Should_return_single_content_when_finding_content()
        {
            var contentId = Guid.NewGuid();
            var contentEntity = CreateContent(contentId, Guid.Empty, Guid.Empty);

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
                      myString {{
                        iv
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
                    }}
                  }}
                }}";

            A.CallTo(() => contentRepository.FindContentAsync(appEntity, schemaId, contentId))
                .Returns(Task.FromResult(contentEntity));

            var result = await sut.QueryAsync(appEntity, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = new
                    {
                        id = contentEntity.Id,
                        version = 1,
                        created = contentEntity.Created.ToDateTimeUtc(),
                        createdBy = "subject:user1",
                        lastModified = contentEntity.LastModified.ToDateTimeUtc(),
                        lastModifiedBy = "subject:user2",
                        url = $"contents/my-schema/{contentEntity.Id}",
                        data = new
                        {
                            myString = new
                            {
                                iv = "value"
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
                                iv = contentEntity.LastModified.ToDateTimeUtc()
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
                            }
                        }
                    }
                }
            };

            AssertJson(expected, new { data = result.Data });
        }

        [Fact]
        public async Task Should_also_fetch_referenced_contents_when_field_is_included_in_query()
        {
            var contentRefId = Guid.NewGuid();
            var contentRefEntity = CreateContent(contentRefId, Guid.Empty, Guid.Empty);

            var contentId = Guid.NewGuid();
            var contentEntity = CreateContent(contentId, contentRefId, Guid.Empty);

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

            var refContents = new List<IContentEntity> { contentRefEntity };

            A.CallTo(() => contentRepository.FindContentAsync(appEntity, schemaId, contentId))
                .Returns(Task.FromResult(contentEntity));

            A.CallTo(() => contentRepository.QueryAsync(appEntity, schemaId, false, A<HashSet<Guid>>.That.Matches(x => x.Contains(contentRefId)), null))
                .Returns(Task.FromResult<IReadOnlyList<IContentEntity>>(refContents));

            var result = await sut.QueryAsync(appEntity, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = new
                    {
                        id = contentEntity.Id,
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

            AssertJson(expected, new { data = result.Data });
        }

        [Fact]
        public async Task Should_also_fetch_referenced_assets_when_field_is_included_in_query()
        {
            var assetRefId = Guid.NewGuid();
            var assetRefEntity = CreateAsset(assetRefId);

            var contentId = Guid.NewGuid();
            var contentEntity = CreateContent(contentId, Guid.Empty, assetRefId);

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

            var refAssets = new List<IAssetEntity> { assetRefEntity };

            A.CallTo(() => contentRepository.FindContentAsync(appEntity, schemaId, contentId))
                .Returns(Task.FromResult(contentEntity));

            A.CallTo(() => assetRepository.QueryAsync(appEntity.Id, null, A<HashSet<Guid>>.That.Matches(x => x.Contains(assetRefId)), null, int.MaxValue, 0))
                .Returns(Task.FromResult<IReadOnlyList<IAssetEntity>>(refAssets));

            var result = await sut.QueryAsync(appEntity, new GraphQLQuery { Query = query });

            var expected = new
            {
                data = new
                {
                    findMySchemaContent = new
                    {
                        id = contentEntity.Id,
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

            AssertJson(expected, new { data = result.Data });
        }

        private static IContentEntity CreateContent(Guid id, Guid refId, Guid assetId)
        {
            var now = DateTime.UtcNow.ToInstant();

            var data =
                new NamedContentData()
                    .AddField("my-json",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new { value = 1 })))
                    .AddField("my-string",
                        new ContentFieldData().AddValue("iv", "value"))
                    .AddField("my-assets",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new[] { assetId })))
                    .AddField("my-number",
                        new ContentFieldData().AddValue("iv", 1))
                    .AddField("my-boolean",
                        new ContentFieldData().AddValue("iv", true))
                    .AddField("my-datetime",
                        new ContentFieldData().AddValue("iv", now.ToDateTimeUtc()))
                    .AddField("my-references",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new[] { refId })))
                    .AddField("my-geolocation",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new { latitude = 10, longitude = 20 })));

            var contentEntity = new FakeContentEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken("subject", "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken("subject", "user2"),
                Data = data
            };

            return contentEntity;
        }

        private static IAssetEntity CreateAsset(Guid id)
        {
            var now = DateTime.UtcNow.ToInstant();

            var assetEntity = new FakeAssetEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken("subject", "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken("subject", "user2"),
                FileName = "MyFile.png",
                FileSize = 1024,
                FileVersion = 123,
                MimeType = "image/png",
                IsImage = true,
                PixelWidth = 800,
                PixelHeight = 600
            };

            return assetEntity;
        }

        private static void AssertJson(object expected, object result)
        {
            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            var expectJson = JsonConvert.SerializeObject(expected, Formatting.Indented);

            Assert.Equal(expectJson, resultJson);
        }
    }
}
