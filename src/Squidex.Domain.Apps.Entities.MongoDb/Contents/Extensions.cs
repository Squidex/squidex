// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public static class Extensions
    {
        private const int MaxLength = 1024 * 1024;

        public static List<Guid> ToReferencedIds(this IdContentData data, Schema schema)
        {
            return data.GetReferencedIds(schema).ToList();
        }

        public static NamedContentData FromMongoModel(this IdContentData result, Schema schema, List<Guid> deletedIds)
        {
            return result.ConvertId2Name(schema,
                FieldConverters.ForValues(
                    ValueConverters.DecodeJson(),
                    ValueReferencesConverter.CleanReferences(deletedIds)),
                FieldConverters.ForNestedId2Name(
                    ValueConverters.DecodeJson(),
                    ValueReferencesConverter.CleanReferences(deletedIds)));
        }

        public static IdContentData ToMongoModel(this NamedContentData result, Schema schema)
        {
            return result.ConvertName2Id(schema,
                FieldConverters.ForValues(
                    ValueConverters.EncodeJson()),
                FieldConverters.ForNestedName2Id(
                    ValueConverters.EncodeJson()));
        }

        public static string ToFullText<T>(this ContentData<T> data)
        {
            var stringBuilder = new StringBuilder();

            foreach (var text in data.Values.SelectMany(x => x.Values).Where(x => x != null).OfType<JValue>())
            {
                if (text.Type == JTokenType.String)
                {
                    var value = text.ToString(CultureInfo.InvariantCulture);

                    if (value.Length < 1000)
                    {
                        if (stringBuilder.Length > 0)
                        {
                            stringBuilder.Append(" ");
                        }

                        stringBuilder.Append(text);
                    }
                }
            }

            var result = stringBuilder.ToString();

            if (result.Length > MaxLength)
            {
                result = result.Substring(MaxLength);
            }

            return result;
        }
    }
}
