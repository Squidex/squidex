// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace Squidex.Domain.Apps.Entities.MongoDb
{
    internal sealed class MongoCountEntity
    {
        [BsonId]
        [BsonRequired]
        public string Key { get; set; }

        [BsonElement]
        public long Count { get; set; }

        [BsonElement]
        public Instant Created { get; set; }
    }
}
