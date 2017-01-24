// ==========================================================================
//  MongoAppContributorEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Squidex.Core.Apps;
using Squidex.Read.Apps;

namespace Squidex.Read.MongoDb.Apps
{
    public sealed class MongoAppContributorEntity : IAppContributorEntity
    {
        [BsonRequired]
        [BsonElement]
        public string ContributorId { get; set; }

        [BsonRequired]
        [BsonElement]
        public PermissionLevel Permission { get; set; }
    }
}
