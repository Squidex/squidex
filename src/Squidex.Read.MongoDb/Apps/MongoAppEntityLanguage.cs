// ==========================================================================
//  MongoAppEntityLanguage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Read.MongoDb.Apps
{
    public sealed class MongoAppEntityLanguage
    {
        [BsonRequired]
        [BsonElement]
        public string Iso2Code { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsOptional { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<string> Fallback { get; set; }
    }
}
