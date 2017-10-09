// ==========================================================================
//  MongoAppEntityContributor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Read.MongoDb.Apps
{
    public sealed class MongoAppEntityContributor : IAppContributorEntity
    {
        [BsonRequired]
        [BsonElement]
        public string ContributorId { get; set; }

        [BsonRequired]
        [BsonElement]
        public AppContributorPermission Permission { get; set; }
    }
}
