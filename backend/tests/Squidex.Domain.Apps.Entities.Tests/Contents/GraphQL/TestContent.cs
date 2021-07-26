// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public static class TestContent
    {
        public const string AllFields = @"
            id
            version
            created
            createdBy
            createdByUser {
              id
              email
              displayName
            }
            lastModified
            lastModifiedBy
            lastModifiedByUser {
              id
              email
              displayName
            }
            status
            statusColor
            newStatus
            newStatusColor
            url
            data {
              myJson {
                iv
                ivValue: iv(path: ""value"")
              }
              myString {
                iv
              }
              myLocalizedString {
                de_DE
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
              myGeolocation {
                iv
              }
              myComponent__Dynamic {
                iv
              }
              myComponent {
                iv {
                  schemaId
                  schemaRef1Field
                }
              }
              myComponents__Dynamic {
                iv
              }
              myComponents {
                iv {
                  __typename
                  ... on MyRefSchema1Component {
                    schemaId
                    schemaRef1Field
                  }
                  ... on MyRefSchema2Component {
                    schemaId
                    schemaRef2Field
                  }
                }
              }
              myTags {
                iv
              }
              myArray {
                iv {
                  nestedNumber
                  nestedBoolean
                }
              }
            }";

        public const string AllFlatFields = @"
            id
            version
            created
            createdBy
            createdByUser {
              id
              email
              displayName
            }
            lastModified
            lastModifiedBy
            lastModifiedByUser {
              id
              email
              displayName
            }
            status
            statusColor
            newStatus
            newStatusColor
            url
            flatData {
              myJson
              myJsonValue: myJson(path: ""value"")
              myString
              myLocalizedString
              myNumber
              myBoolean
              myDatetime
              myGeolocation
              myComponent__Dynamic
              myComponent {
                schemaId
                schemaRef1Field
              }
              myComponents__Dynamic
              myComponents {
                __typename
                ... on MyRefSchema1Component {
                  schemaId
                  schemaRef1Field
                }
                ... on MyRefSchema2Component {
                  schemaId
                  schemaRef2Field
                }
              }
              myTags
              myArray {
                nestedNumber
                nestedBoolean
              }
            }";

        public static IEnrichedContentEntity Create(DomainId id, DomainId refId = default, DomainId assetId = default, ContentData? data = null)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            data ??=
                new ContentData()
                    .AddField("my-localized-string",
                        new ContentFieldData()
                            .AddLocalized("de-DE", "de-DE"))
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddInvariant(null))
                    .AddField("my-assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId.ToString())))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddInvariant(1.0))
                    .AddField("my-boolean",
                        new ContentFieldData()
                            .AddInvariant(true))
                    .AddField("my-datetime",
                        new ContentFieldData()
                            .AddInvariant(now))
                    .AddField("my-tags",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array("tag1", "tag2")))
                    .AddField("my-references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(refId.ToString())))
                    .AddField("my-union",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(refId.ToString())))
                    .AddField("my-geolocation",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("latitude", 10)
                                    .Add("longitude", 20)))
                    .AddField("my-component",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add(Component.Discriminator, TestSchemas.Ref1.Id)
                                    .Add("schemaRef1Field", "Component1")))
                    .AddField("my-components",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Array(
                                    JsonValue.Object()
                                        .Add(Component.Discriminator, TestSchemas.Ref1.Id)
                                        .Add("schemaRef1Field", "Component1"),
                                    JsonValue.Object()
                                        .Add(Component.Discriminator, TestSchemas.Ref2.Id)
                                        .Add("schemaRef2Field", "Component2"))))
                    .AddField("my-json",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("value", 1)))
                    .AddField("my-array",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested-number", 10)
                                    .Add("nested-boolean", true),
                                JsonValue.Object()
                                    .Add("nested-number", 20)
                                    .Add("nested-boolean", false))));

            var content = new ContentEntity
            {
                Id = id,
                AppId = TestApp.DefaultId,
                Version = 1,
                Created = now,
                CreatedBy = RefToken.User("user1"),
                LastModified = now,
                LastModifiedBy = RefToken.Client("client1"),
                Data = data,
                SchemaId = TestSchemas.DefaultId,
                Status = Status.Draft,
                StatusColor = "red",
                NewStatus = Status.Published,
                NewStatusColor = "blue"
            };

            return content;
        }

        public static IEnrichedContentEntity CreateRef(NamedId<DomainId> schemaId, DomainId id, string field, string value)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var data =
                new ContentData()
                    .AddField(field,
                        new ContentFieldData()
                            .AddInvariant(value));

            var content = new ContentEntity
            {
                Id = id,
                AppId = TestApp.DefaultId,
                Version = 1,
                Created = now,
                CreatedBy = RefToken.User("user1"),
                LastModified = now,
                LastModifiedBy = RefToken.User("user2"),
                Data = data,
                SchemaId = schemaId,
                Status = Status.Draft,
                StatusColor = "red",
                NewStatus = Status.Published,
                NewStatusColor = "blue"
            };

            return content;
        }

        public static object Response(IEnrichedContentEntity content)
        {
            return new
            {
                id = content.Id,
                version = 1,
                created = content.Created,
                createdBy = content.CreatedBy.ToString(),
                createdByUser = new
                {
                    id = content.CreatedBy.Identifier,
                    email = $"{content.CreatedBy.Identifier}@email.com",
                    displayName = $"name_{content.CreatedBy.Identifier}"
                },
                lastModified = content.LastModified,
                lastModifiedBy = content.LastModifiedBy.ToString(),
                lastModifiedByUser = new
                {
                    id = content.LastModifiedBy.Identifier,
                    email = $"{content.LastModifiedBy}",
                    displayName = content.LastModifiedBy.Identifier
                },
                status = "DRAFT",
                statusColor = "red",
                newStatus = "PUBLISHED",
                newStatusColor = "blue",
                url = $"contents/my-schema/{content.Id}",
                data = Data(content)
            };
        }

        public static object FlatResponse(IEnrichedContentEntity content)
        {
            return new
            {
                id = content.Id,
                version = 1,
                created = content.Created,
                createdBy = content.CreatedBy.ToString(),
                createdByUser = new
                {
                    id = content.CreatedBy.Identifier,
                    email = $"{content.CreatedBy.Identifier}@email.com",
                    displayName = $"name_{content.CreatedBy.Identifier}"
                },
                lastModified = content.LastModified,
                lastModifiedBy = content.LastModifiedBy.ToString(),
                lastModifiedByUser = new
                {
                    id = content.LastModifiedBy.Identifier,
                    email = $"{content.LastModifiedBy}",
                    displayName = content.LastModifiedBy.Identifier
                },
                status = "DRAFT",
                statusColor = "red",
                newStatus = "PUBLISHED",
                newStatusColor = "blue",
                url = $"contents/my-schema/{content.Id}",
                flatData = FlatData(content)
            };
        }

        public static object Input(IContentEntity content, DomainId refId = default, DomainId assetId = default)
        {
            var result = new Dictionary<string, object>
            {
                ["myJson"] = new
                {
                    iv = new
                    {
                        value = 1
                    },
                },
                ["myString"] = new
                {
                    iv = (string?)null,
                },
                ["myLocalizedString"] = new
                {
                    de_DE = "de-DE"
                },
                ["myNumber"] = new
                {
                    iv = 1.0
                },
                ["myBoolean"] = new
                {
                    iv = true
                },
                ["myDatetime"] = new
                {
                    iv = content.LastModified.ToString()
                },
                ["myGeolocation"] = new
                {
                    iv = new
                    {
                        latitude = 10,
                        longitude = 20
                    }
                },
                ["myComponent"] = new
                {
                    iv = new Dictionary<string, object>
                    {
                        ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                        ["schemaRef1Field"] = "Component1"
                    }
                },
                ["myComponents"] = new
                {
                    iv = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                            ["schemaRef1Field"] = "Component1"
                        },
                        new Dictionary<string, object>
                        {
                            ["schemaId"] = TestSchemas.Ref2.Id.ToString(),
                            ["schemaRef2Field"] = "Component2"
                        },
                    }
                },
                ["myTags"] = new
                {
                    iv = new[]
                    {
                        "tag1",
                        "tag2"
                    }
                },
                ["myArray"] = new
                {
                    iv = new[]
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
            };

            if (refId != default)
            {
                result["myReferences"] = new
                {
                    iv = new[]
                    {
                        refId
                    }
                };

                result["myUnion"] = new
                {
                    iv = new[]
                    {
                        refId
                    }
                };
            }

            if (assetId != default)
            {
                result["myAssets"] = new
                {
                    iv = new[]
                    {
                        assetId
                    }
                };
            }

            return result;
        }

        private static object Data(IContentEntity content)
        {
            var result = new Dictionary<string, object>
            {
                ["myJson"] = new
                {
                    iv = new
                    {
                        value = 1
                    },
                    ivValue = 1
                },
                ["myString"] = new
                {
                    iv = (string?)null,
                },
                ["myLocalizedString"] = new
                {
                    de_DE = "de-DE"
                },
                ["myNumber"] = new
                {
                    iv = 1.0
                },
                ["myBoolean"] = new
                {
                    iv = true
                },
                ["myDatetime"] = new
                {
                    iv = content.LastModified.ToString()
                },
                ["myGeolocation"] = new
                {
                    iv = new
                    {
                        latitude = 10,
                        longitude = 20
                    }
                },
                ["myComponent__Dynamic"] = new
                {
                    iv = new Dictionary<string, object>
                    {
                        ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                        ["schemaRef1Field"] = "Component1"
                    }
                },
                ["myComponent"] = new
                {
                    iv = new Dictionary<string, object>
                    {
                        ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                        ["schemaRef1Field"] = "Component1"
                    }
                },
                ["myComponents__Dynamic"] = new
                {
                    iv = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                            ["schemaRef1Field"] = "Component1"
                        },
                        new Dictionary<string, object>
                        {
                            ["schemaId"] = TestSchemas.Ref2.Id.ToString(),
                            ["schemaRef2Field"] = "Component2"
                        },
                    }
                },
                ["myComponents"] = new
                {
                    iv = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "MyRefSchema1Component",
                            ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                            ["schemaRef1Field"] = "Component1"
                        },
                        new Dictionary<string, object>
                        {
                            ["__typename"] = "MyRefSchema2Component",
                            ["schemaId"] = TestSchemas.Ref2.Id.ToString(),
                            ["schemaRef2Field"] = "Component2"
                        },
                    }
                },
                ["myTags"] = new
                {
                    iv = new[]
                    {
                        "tag1",
                        "tag2"
                    }
                },
                ["myArray"] = new
                {
                    iv = new[]
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
            };

            return result;
        }

        private static object FlatData(IContentEntity content)
        {
            var result = new Dictionary<string, object?>
            {
                ["myJson"] = new
                {
                    value = 1
                },
                ["myJsonValue"] = 1,
                ["myString"] = null,
                ["myLocalizedString"] = "de-DE",
                ["myNumber"] = 1.0,
                ["myBoolean"] = true,
                ["myDatetime"] = content.LastModified.ToString(),
                ["myGeolocation"] = new
                {
                    latitude = 10,
                    longitude = 20
                },
                ["myComponent__Dynamic"] = new Dictionary<string, object>
                {
                    ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                    ["schemaRef1Field"] = "Component1"
                },
                ["myComponent"] = new Dictionary<string, object>
                {
                    ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                    ["schemaRef1Field"] = "Component1"
                },
                ["myComponents__Dynamic"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                        ["schemaRef1Field"] = "Component1"
                    },
                    new Dictionary<string, object>
                    {
                        ["schemaId"] = TestSchemas.Ref2.Id.ToString(),
                        ["schemaRef2Field"] = "Component2"
                    },
                },
                ["myComponents"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["__typename"] = "MyRefSchema1Component",
                        ["schemaId"] = TestSchemas.Ref1.Id.ToString(),
                        ["schemaRef1Field"] = "Component1"
                    },
                    new Dictionary<string, object>
                    {
                        ["__typename"] = "MyRefSchema2Component",
                        ["schemaId"] = TestSchemas.Ref2.Id.ToString(),
                        ["schemaRef2Field"] = "Component2"
                    },
                },
                ["myTags"] = new[]
                {
                    "tag1",
                    "tag2"
                },
                ["myArray"] = new[]
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
            };

            return result;
        }
    }
}
