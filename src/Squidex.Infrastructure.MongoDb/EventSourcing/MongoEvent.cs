// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
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
        public JToken Metadata { get; set; }

        public static MongoEvent FromEventData(EventData data)
        {
            return new MongoEvent { Type = data.Type, Metadata = data.Metadata, Payload = data.Payload.ToString() };
        }

        public EventData ToEventData()
        {
            return new EventData { Type = Type, Metadata = Metadata, Payload = JObject.Parse(Payload) };
        }
    }
}