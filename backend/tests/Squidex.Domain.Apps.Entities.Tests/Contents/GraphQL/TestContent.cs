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
            lastModified
            lastModifiedBy
            status
            statusColor
            url
            data {
                gql_2Numbers {
                    iv
                }
                gql_2Numbers2 {
                    iv
                }
                myString {
                    de
                }
                myString2 {
                    iv
                }
                myNumber {
                    iv
                }
                myNumber2 {
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
                        nestedNumber2
                        nestedBoolean
                    }
                }
            }";

        public static IEnrichedContentEntity Create(NamedId<DomainId> appId, NamedId<DomainId> schemaId, DomainId id, DomainId refId, DomainId assetId, ContentData? data = null)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            data ??=
                new ContentData()
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddLocalized("de", "value"))
                    .AddField("my-string2",
                        new ContentFieldData()
                            .AddInvariant(null))
                    .AddField("my-assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId.ToString())))
                    .AddField("2_numbers",
                        new ContentFieldData()
                            .AddInvariant(22))
                    .AddField("2-numbers",
                        new ContentFieldData()
                            .AddInvariant(23))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddInvariant(1.0))
                    .AddField("my_number",
                        new ContentFieldData()
                            .AddInvariant(null))
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
                            .AddInvariant(JsonValue.Object().Add("latitude", 10).Add("longitude", 20)))
                    .AddField("my-json",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Object().Add("value", 1)))
                    .AddField("my-localized",
                        new ContentFieldData()
                            .AddLocalized("de-DE", "de-DE"))
                    .AddField("my-array",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested-number", 10)
                                    .Add("nested_number", null)
                                    .Add("nested-boolean", true),
                                JsonValue.Object()
                                    .Add("nested-number", 20)
                                    .Add("nested_number", null)
                                    .Add("nested-boolean", false))));

            var content = new ContentEntity
            {
                Id = id,
                AppId = appId,
                Version = 1,
                Created = now,
                CreatedBy = RefToken.User("user1"),
                LastModified = now,
                LastModifiedBy = RefToken.User("user2"),
                Data = data,
                SchemaId = schemaId,
                Status = Status.Draft,
                StatusColor = "red"
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
                Version = 1,
                Created = now,
                CreatedBy = RefToken.User("user1"),
                LastModified = now,
                LastModifiedBy = RefToken.User("user2"),
                Data = data,
                SchemaId = schemaId,
                Status = Status.Draft,
                StatusColor = "red"
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
                createdBy = "subject:user1",
                lastModified = content.LastModified,
                lastModifiedBy = "subject:user2",
                status = "DRAFT",
                statusColor = "red",
                url = $"contents/my-schema/{content.Id}",
                data = Data(content)
            };
        }

        public static object Data(IContentEntity content, DomainId refId = default, DomainId assetId = default)
        {
            var result = new Dictionary<string, object>
            {
                ["gql_2Numbers"] = new
                {
                    iv = 22.0
                },
                ["gql_2Numbers2"] = new
                {
                    iv = 23.0
                },
                ["myString"] = new
                {
                    de = "value"
                },
                ["myString2"] = new
                {
                    iv = (object?)null
                },
                ["myNumber"] = new
                {
                    iv = 1.0
                },
                ["myNumber2"] = new
                {
                    iv = (object?)null
                },
                ["myBoolean"] = new
                {
                    iv = true
                },
                ["myDatetime"] = new
                {
                    iv = content.LastModified.ToString()
                },
                ["myJson"] = new
                {
                    iv = new
                    {
                        value = 1
                    }
                },
                ["myGeolocation"] = new
                {
                    iv = new
                    {
                        latitude = 10,
                        longitude = 20
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
                ["myLocalized"] = new
                {
                    de_DE = "de-DE"
                },
                ["myArray"] = new
                {
                    iv = new[]
                    {
                        new
                        {
                            nestedNumber = 10.0,
                            nestedNumber2 = (object?)null,
                            nestedBoolean = true
                        },
                        new
                        {
                            nestedNumber = 20.0,
                            nestedNumber2 = (object?)null,
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
    }
}
