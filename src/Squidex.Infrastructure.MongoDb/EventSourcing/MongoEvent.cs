// ==========================================================================
//  MongoEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing
{
    public class MongoEvent
    {
        [BsonElement]
        [BsonRequired]
        public Guid EventId { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Payload { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Metadata { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Type { get; set; }

        public MongoEvent()
        {
        }

        public MongoEvent(EventData data)
        {
            SimpleMapper.Map(data, this);
        }

        public EventData ToEventData()
        {
            return SimpleMapper.Map(this, new EventData());
        }
    }
}