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
        public List<MongoAppClientEntity> Clients { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<MongoAppContributorEntity> Contributors { get; set; }

        IEnumerable<Language> IAppEntity.Languages
        {
            get { return Languages.Select(Language.GetLanguage); }
        }

        IEnumerable<IAppClientEntity> IAppEntity.Clients
        {
            get { return Clients; }
        }

        IEnumerable<IAppContributorEntity> IAppEntity.Contributors
        {
            get { return Contributors; }
        }

        public MongoAppEntity()
        {
            Contributors = new List<MongoAppContributorEntity>();

            Clients = new List<MongoAppClientEntity>();
        }
    }
}
