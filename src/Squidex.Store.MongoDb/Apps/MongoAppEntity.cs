// ==========================================================================
//  MongoAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;
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
        public List<string> Languages { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<MongoAppClientKeyEntity> ClientKeys { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<MongoAppContributorEntity> Contributors { get; set; }

        IEnumerable<Language> IAppEntity.Languages
        {
            get { return Languages.Select(Language.GetLanguage); }
        }

        IEnumerable<IAppClientKeyEntity> IAppEntity.ClientKeys
        {
            get { return ClientKeys; }
        }

        IEnumerable<IAppContributorEntity> IAppEntity.Contributors
        {
            get { return Contributors; }
        }

        public MongoAppEntity()
        {
            Contributors = new List<MongoAppContributorEntity>();

            ClientKeys = new List<MongoAppClientKeyEntity>();
        }
    }
}
