// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations
{
    public static class Extensions
    {
        public static NamedContentData FromMongoModel(this IdContentData result, Schema schema, IJsonSerializer serializer)
        {
            return result.ConvertId2Name(schema,
                FieldConverters.ForValues(
                    ValueConverters.DecodeJson(serializer)),
                FieldConverters.ForNestedId2Name(
                    ValueConverters.DecodeJson(serializer)));
        }

        public static IdContentData ToMongoModel(this NamedContentData result, Schema schema, IJsonSerializer serializer)
        {
            return result.ConvertName2Id(schema,
                FieldConverters.ForValues(
                    ValueConverters.EncodeJson(serializer)),
                FieldConverters.ForNestedName2Id(
                    ValueConverters.EncodeJson(serializer)));
        }

        public static bool HasStatus(this MongoContentEntity content, Status[]? status)
        {
            return status == null || status.Contains(content.Status);
        }
    }
}
