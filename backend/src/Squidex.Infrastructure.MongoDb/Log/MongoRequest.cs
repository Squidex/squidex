﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace Squidex.Infrastructure.Log
{
    public sealed class MongoRequest
    {
        [BsonId]
        [BsonElement]
        public ObjectId Id { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Key { get; set; }

        [BsonElement]
        [BsonRequired]
        public Instant Timestamp { get; set; }

        [BsonElement]
        [BsonRequired]
        public Dictionary<string, string> Properties { get; set; }
    }
}
