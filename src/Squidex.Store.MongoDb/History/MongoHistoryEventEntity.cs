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
using Squidex.Infrastructure.CQRS;
using Squidex.Store.MongoDb.Utils;

namespace Squidex.Store.MongoDb.History
{
    public sealed class MongoHistoryEventEntity : MongoEntity
    {
        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Channel { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Message { get; set; }

        [BsonRequired]
        [BsonElement]
        public UserToken User { get; set; }

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, string> Parameters { get; set; }

        public MongoHistoryEventEntity()
        {
            Parameters = new Dictionary<string, string>();
        }

        public MongoHistoryEventEntity Setup<T>(EnvelopeHeaders headers, string channel)
        {
            Channel = channel;

            if (headers.Contains(CommonHeaders.User))
            {
                AddParameter("User", headers[CommonHeaders.User].ToString());
            }

            Message = TypeNameRegistry.GetName<T>();

            return this;
        }

        public MongoHistoryEventEntity AddParameter(string key, string value)
        {
            Parameters.Add(key, value);

            return this;
        }
    }
}
