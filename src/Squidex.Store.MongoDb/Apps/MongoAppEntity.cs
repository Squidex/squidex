// ==========================================================================
//  MongoAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Read.Apps;
using Squidex.Store.MongoDb.Utils;

namespace Squidex.Store.MongoDb.Apps
{
    public sealed class MongoAppEntity : MongoEntity, IAppEntity
    {
        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<MongoAppContributorEntity> Contributors { get; set; }

        IEnumerable<IAppContributorEntity> IAppEntity.Contributors
        {
            get { return Contributors; }
        }

        public MongoAppEntity()
        {
            Contributors = new List<MongoAppContributorEntity>();
        }
    }
}
