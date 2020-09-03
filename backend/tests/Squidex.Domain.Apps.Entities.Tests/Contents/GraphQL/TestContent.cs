﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
            }";

        public static IEnrichedContentEntity Create(NamedId<Guid> schemaId, Guid id, Guid refId, Guid assetId, NamedContentData? data = null)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            data ??=
                new NamedContentData()
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddValue("de", "value"))
                    .AddField("my-assets",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(assetId.ToString())))
                    .AddField("2_numbers",
                        new ContentFieldData()
                            .AddValue("iv", 22))
                    .AddField("2-numbers",
                        new ContentFieldData()
                            .AddValue("iv", 23))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddValue("iv", 1.0))
                    .AddField("my_number",
                        new ContentFieldData()
                            .AddValue("iv", 2.0))
                    .AddField("my-boolean",
                        new ContentFieldData()
                            .AddValue("iv", true))
                    .AddField("my-datetime",
                        new ContentFieldData()
                            .AddValue("iv", now))
                    .AddField("my-tags",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array("tag1", "tag2")))
                    .AddField("my-references",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(refId.ToString())))
                    .AddField("my-union",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(refId.ToString())))
                    .AddField("my-geolocation",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Object().Add("latitude", 10).Add("longitude", 20)))
                    .AddField("my-json",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Object().Add("value", 1)))
                    .AddField("my-localized",
                        new ContentFieldData()
                            .AddValue("de-DE", "de-DE"))
                    .AddField("my-array",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested-boolean", true)
                                    .Add("nested-number", 10)
                                    .Add("nested_number", 11),
                                JsonValue.Object()
                                    .Add("nested-boolean", false)
                                    .Add("nested-number", 20)
                                    .Add("nested_number", 21))));

            var content = new ContentEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken(RefTokenType.Subject, "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken(RefTokenType.Subject, "user2"),
                Data = data,
                SchemaId = schemaId,
                Status = Status.Draft,
                StatusColor = "red"
            };

            return content;
        }

        public static IEnrichedContentEntity CreateRef(NamedId<Guid> schemaId, Guid id, string field, string value)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var data =
                new NamedContentData()
                    .AddField(field,
                        new ContentFieldData()
                            .AddValue("iv", value));

            var content = new ContentEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken(RefTokenType.Subject, "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken(RefTokenType.Subject, "user2"),
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
            };
        }
    }
}
