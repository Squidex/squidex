// ==========================================================================
//  MongoSchemaEntity.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PinkParrot.Core.Schemas;
using PinkParrot.Read.Schemas.Repositories;
using PinkParrot.Store.MongoDb.Schemas.Models;
using PinkParrot.Store.MongoDb.Utils;

namespace PinkParrot.Store.MongoDb.Schemas
{
    public sealed class MongoSchemaEntity : MongoEntity, ISchemaEntityWithSchema
    {
        private Schema schema;

        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }
        
        [BsonRequired]
        [BsonElement]
        public bool IsDeleted { get; set; }

        [BsonRequired]
        [BsonElement]
        public BsonDocument Schema { get; set; }

        Schema ISchemaEntityWithSchema.Schema
        {
            get { return schema; }
        }

        public void DeserializeSchema(JsonSerializerSettings serializerSettings, FieldRegistry fieldRegistry)
        {
            if (schema != null)
            {
                return;
            }

            var dto = Schema.ToJsonObject<SchemaDto>(serializerSettings);

            schema = dto?.ToSchema(fieldRegistry);
        }
    }
}
