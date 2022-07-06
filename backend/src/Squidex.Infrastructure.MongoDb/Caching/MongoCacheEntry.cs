// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.Caching
{
    public sealed class MongoCacheEntry
    {
        [BsonId]
        public string Key { get; set; }

        [BsonElement]
        public DateTime Expires { get; set; }

        [BsonElement]
        public byte[] Value { get; set; }
    }
}
