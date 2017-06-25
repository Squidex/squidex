// ==========================================================================
//  MongoContentEntity.cs
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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using Squidex.Core;
using Squidex.Core.Contents;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Contents;
using JsonConvert = Newtonsoft.Json.JsonConvert;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Read.MongoDb.Contents
{
    public sealed class MongoContentEntity : MongoEntity, IContentEntity
    {
        private static readonly JsonWriterSettings Settings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
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
        public Guid SchemaId { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken LastModifiedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public BsonDocument Data { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<Guid> ReferencedIds { get; set; } = new List<Guid>();

        [BsonRequired]
        [BsonElement]
        public List<Guid> ReferencedIdsDeleted { get; set; } = new List<Guid>();

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
                var jsonString = Data.ToJson(Settings);

                contentData = JsonConvert.DeserializeObject<ContentData>(jsonString);
                contentData = contentData.ToCleanedReferences(schema, new HashSet<Guid>(ReferencedIdsDeleted));
                contentData = contentData.ToNameModel(schema, true);
            }
            else
            {
                contentData = null;
            }
        }

        public void SetData(Schema schema, ContentData newContentData)
        {
            newContentData = newContentData?.ToIdModel(schema, true);

            if (newContentData != null)
            {
                var jsonString = JsonConvert.SerializeObject(newContentData);

                Data = BsonDocument.Parse(jsonString);
            }
            else
            {
                Data = null;
            }

            ReferencedIds = newContentData?.GetReferencedIds(schema).ToList() ?? new List<Guid>();

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
