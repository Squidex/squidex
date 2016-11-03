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

namespace Squidex.Store.MongoDb.Apps
{
    public sealed class MongoAppContributorEntity : IAppContributorEntity
    {
        [BsonRequired]
        [BsonElement]
        public string SubjectId { get; set; }

        [BsonRequired]
        [BsonElement]
        public PermissionLevel Permission { get; set; }
    }
}
