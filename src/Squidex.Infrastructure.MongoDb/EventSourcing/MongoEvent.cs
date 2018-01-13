// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.EventSourcing
{
    public class MongoEvent
    {
        [BsonElement]
        [BsonRequired]
        public JToken Payload { get; set; }

        [BsonElement]
        [BsonRequired]
        public JToken Metadata { get; set; }

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