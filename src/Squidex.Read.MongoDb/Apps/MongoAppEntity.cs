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
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Apps;

namespace Squidex.Read.MongoDb.Apps
{
    public sealed class MongoAppEntity : MongoEntity, IAppEntity
    {
        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public string MasterLanguage { get; set; }

        [BsonRequired]
        [BsonElement]
        public HashSet<string> Languages { get; } = new HashSet<string>(); 

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppClientEntity> Clients { get; } = new Dictionary<string, MongoAppClientEntity>();

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppContributorEntity> Contributors { get; } = new Dictionary<string, MongoAppContributorEntity>();

        IReadOnlyCollection<IAppClientEntity> IAppEntity.Clients
        {
            get { return Clients.Values; }
        }

        IReadOnlyCollection<IAppContributorEntity> IAppEntity.Contributors
        {
            get { return Contributors.Values; }
        }

        IReadOnlyCollection<Language> IAppEntity.Languages
        {
            get { return Languages.Select(Language.GetLanguage).ToList(); }
        }

        Language IAppEntity.MasterLanguage
        {
            get { return Language.GetLanguage(MasterLanguage); }
        }
    }
}
