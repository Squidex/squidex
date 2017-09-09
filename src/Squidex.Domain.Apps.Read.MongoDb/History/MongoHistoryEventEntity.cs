// ==========================================================================
//  MongoHistoryEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.History
{
    public sealed class MongoHistoryEventEntity : MongoEntity, IAppRefEntity, IEntityWithCreatedBy
    {
        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Channel { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Message { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken Actor { get; set; }

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, string> Parameters { get; set; }

        RefToken IEntityWithCreatedBy.CreatedBy
        {
            get
            {
                return Actor;
            }
            set
            {
                Actor = value;
            }
        }

        public MongoHistoryEventEntity()
        {
            Parameters = new Dictionary<string, string>();
        }

        public MongoHistoryEventEntity AddParameter(string key, string value)
        {
            Parameters.Add(key, value);

            return this;
        }
    }
}
