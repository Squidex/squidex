// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class MongoEvent
    {
        [BsonElement]
        [BsonRequired]
        public string Type { get; set; }

        [BsonJson]
        [BsonRequired]
        public string Payload { get; set; }

        [BsonElement]
        [BsonRequired]
        public BsonDocument Metadata { get; set; }

        public static MongoEvent FromEventData(EventData data)
        {
            return new MongoEvent { Type = data.Type, Metadata = BsonDocument.Parse(data.Payload), Payload = data.Payload };
        }

        public EventData ToEventData()
        {
            return new EventData { Type = Type, Metadata = Metadata.ToJson().ToString(), Payload = Payload };
        }
    }
}