// ==========================================================================
//  MongoContentEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Contents;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Read.MongoDb.Contents
{
    public sealed class MongoContentEntity : MongoEntity, IContentEntity
    {
        private const int MaxLength = 1024 * 1024;
        private ContentData contentData;

        [BsonRequired]
        [BsonElement]
        public bool IsPublished { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Text { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken LastModifiedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public BsonDocument Data { get; set; }

        ContentData IContentEntity.Data
        {
            get
            {
                return contentData;
            }
        }

        public void ParseData(Schema schema)
        {
            if (Data != null)
            {
                contentData = JsonConvert.DeserializeObject<ContentData>(Data.ToJson()).ToNameModel(schema);
            }
            else
            {
                contentData = null;
            }
        }

        public void SetData(Schema schema, ContentData newContentData)
        {
            if (newContentData != null)
            {
                Data = BsonDocument.Parse(JsonConvert.SerializeObject(newContentData.ToIdModel(schema)));
            }
            else
            {
                Data = null;
            }

            Text = ExtractText(newContentData);
        }

        private static string ExtractText(ContentData data)
        {
            if (data == null)
            {
                return string.Empty;
            }
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
