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
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using JsonConvert = Newtonsoft.Json.JsonConvert;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
// ReSharper disable InvertIf

namespace Squidex.Domain.Apps.Read.MongoDb.Contents
{
    public sealed class MongoContentEntity : IContentEntity, IMongoEntity
    {
        private static readonly JsonWriterSettings Settings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
        private const int MaxLength = 1024 * 1024;
        private NamedContentData contentData;

        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement("ct")]
        public Instant Created { get; set; }

        [BsonRequired]
        [BsonElement("mt")]
        public Instant LastModified { get; set; }

        [BsonRequired]
        [BsonElement("pu")]
        public bool IsPublished { get; set; }

        [BsonRequired]
        [BsonElement("dt")]
        public string DataText { get; set; }

        [BsonRequired]
        [BsonElement("vs")]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement("ai")]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement("si")]
        public Guid SchemaId { get; set; }

        [BsonRequired]
        [BsonElement("cb")]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement("mb")]
        public RefToken LastModifiedBy { get; set; }

        [BsonRequired]
        [BsonElement("do")]
        public BsonDocument DataObject { get; set; }

        [BsonRequired]
        [BsonElement("rf")]
        public List<Guid> ReferencedIds { get; set; } = new List<Guid>();

        [BsonRequired]
        [BsonElement("rd")]
        public List<Guid> ReferencedIdsDeleted { get; set; } = new List<Guid>();

        NamedContentData IContentEntity.Data
        {
            get
            {
                return contentData;
            }
        }

        public void ParseData(Schema schema)
        {
            if (DataObject != null)
            {
                var jsonString = DataObject.ToJson(Settings);

                contentData =
                    JsonConvert.DeserializeObject<IdContentData>(jsonString)
                        .ToCleanedReferences(schema, new HashSet<Guid>(ReferencedIdsDeleted))
                        .ToNameModel(schema, true);
            }
            else
            {
                contentData = null;
            }
        }

        public void SetData(Schema schema, NamedContentData newContentData)
        {
            if (newContentData != null)
            {
                var idModel = newContentData.ToIdModel(schema, true);

                var jsonString = JsonConvert.SerializeObject(idModel);

                DataObject = BsonDocument.Parse(jsonString);
                DataText = ExtractText(idModel);

                ReferencedIds = idModel.GetReferencedIds(schema).ToList();
            }
            else
            {
                DataObject = null;
                DataText = string.Empty;
            }
        }

        private static string ExtractText(IdContentData data)
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
