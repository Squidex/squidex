// ==========================================================================
//  MongoAppEntityClient.cs
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
    public sealed class MongoAppEntityClient : IAppClientEntity
    {
        [BsonRequired]
        [BsonElement]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Secret { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public AppClientPermission Permission { get; set; }

        string IAppClientEntity.Name
        {
            get { return !string.IsNullOrWhiteSpace(Name) ? Name : Id; }
        }

        public MongoAppEntityClient()
        {
            Permission = AppClientPermission.Editor;
        }
    }
}
