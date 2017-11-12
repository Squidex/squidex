// ==========================================================================
//  MongoAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Apps
{
    public sealed class MongoAppEntity : MongoEntity, IAppEntity
    {
        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public string PlanId { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public string PlanOwner { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public string[] ContributorIds { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonJson]
        public AppClients Clients { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonJson]
        public AppContributors Contributors { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonJson]
        public LanguagesConfig LanguagesConfig { get; set; }
    }
}
