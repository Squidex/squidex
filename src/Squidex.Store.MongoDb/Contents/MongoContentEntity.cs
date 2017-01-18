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
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Contents;
// ReSharper disable InvertIf

namespace Squidex.Store.MongoDb.Contents
{
    public sealed class MongoContentEntity : MongoEntity, IContentEntity
    {
        private BsonDocument data;
        private ContentData contentData;

        [BsonRequired]
        [BsonElement]
        public bool IsDeleted { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsPublished { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Text { get; set; }

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
        public BsonDocument Data
        {
            get { return data; }
            set
            {
                data = value;

                contentData = null;
            }
        }

        ContentData IContentEntity.Data
        {
            get
            {
                if (contentData == null)
                {
                    if (data != null)
                    {
                        contentData = JsonConvert.DeserializeObject<ContentData>(data.ToJson());
                    }
                }

                return contentData;
            }
        }

        public void SetData(ContentData newContentData)
        {
            data = null;

            if (newContentData != null)
            {
                data = BsonDocument.Parse(JsonConvert.SerializeObject(newContentData));
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

            foreach (var text in data.Fields.Values.SelectMany(x => x.ValueByLanguage.Values).Where(x => x != null).OfType<JValue>())
            {
                if (text.Type == JTokenType.String)
                {
                    stringBuilder.Append(text);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
