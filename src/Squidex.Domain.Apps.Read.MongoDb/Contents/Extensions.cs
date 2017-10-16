// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents
{
    public static class Extensions
    {
        private const int MaxLength = 1024 * 1024;

        public static BsonDocument ToBsonDocument(this IdContentData data)
        {
            return (BsonDocument)JToken.FromObject(data).ToBson();
        }

        public static List<Guid> ToReferencedIds(this IdContentData data, Schema schema)
        {
            return data.GetReferencedIds(schema).ToList();
        }

        public static NamedContentData ToData(this BsonDocument document, Schema schema, List<Guid> deletedIds)
        {
            return document
                .ToJson()
                .ToObject<IdContentData>()
                .ToCleanedReferences(schema, new HashSet<Guid>(deletedIds ?? new List<Guid>()))
                .ToNameModel(schema, true);
        }

        public static string ToFullText<T>(this ContentData<T> data)
        {
            var stringBuilder = new StringBuilder();

            foreach (var text in data.Values.SelectMany(x => x.Values).Where(x => x != null).OfType<JValue>())
            {
                if (text.Type == JTokenType.String)
                {
                    var value = text.ToString();

                    if (value.Length < 1000)
                    {
                        stringBuilder.Append(" ");
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
