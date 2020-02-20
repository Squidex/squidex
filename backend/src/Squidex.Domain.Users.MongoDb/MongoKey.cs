// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoKey
    {
        [BsonId]
        public string Id { get; set; }

        [BsonElement]
        public string Key { get; set; }

        [BsonElement]
        public MongoKeyParameters Parameters { get; set; }
    }
}
