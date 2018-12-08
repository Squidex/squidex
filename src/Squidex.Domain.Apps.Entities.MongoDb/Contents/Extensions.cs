// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public static class Extensions
    {
        public static List<Guid> ToReferencedIds(this IdContentData data, Schema schema)
        {
            return data.GetReferencedIds(schema).ToList();
        }

        public static NamedContentData FromMongoModel(this IdContentData result, Schema schema, List<Guid> deletedIds, IJsonSerializer serializer)
        {
            return result.ConvertId2Name(schema,
                FieldConverters.ForValues(
                    ValueConverters.DecodeJson(serializer),
                    ValueReferencesConverter.CleanReferences(deletedIds)),
                FieldConverters.ForNestedId2Name(
                    ValueConverters.DecodeJson(serializer),
                    ValueReferencesConverter.CleanReferences(deletedIds)));
        }

        public static IdContentData ToMongoModel(this NamedContentData result, Schema schema, IJsonSerializer serializer)
        {
            return result.ConvertName2Id(schema,
                FieldConverters.ForValues(
                    ValueConverters.EncodeJson(serializer)),
                FieldConverters.ForNestedName2Id(
                    ValueConverters.EncodeJson(serializer)));
        }
    }
}
