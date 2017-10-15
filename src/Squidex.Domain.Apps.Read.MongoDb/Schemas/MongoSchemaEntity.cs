// ==========================================================================
//  MongoSchemaEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Schemas
{
    public sealed class MongoSchemaEntity : MongoEntity, ISchemaEntity
    {
        private Schema schema;

        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public BsonDocument SchemaDocument { get; set; }

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
        public bool IsPublished { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsDeleted { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string ScriptQuery { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string ScriptCreate { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string ScriptUpdate { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string ScriptDelete { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement]
        public string ScriptChange { get; set; }

        [BsonIgnore]
        public Schema SchemaDef
        {
            get
            {
                if (schema == null)
                {
                    schema = SchemaDocument.ToJson().ToObject<Schema>();
                }

                return schema;
            }
            set
            {
                schema = value;

                SchemaDocument = (BsonDocument)JToken.FromObject(schema).ToBson();
            }
        }
    }
}
