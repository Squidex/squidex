// ==========================================================================
//  MongoAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

// ReSharper disable InvertIf
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Squidex.Domain.Apps.Read.MongoDb.Apps
{
    public sealed class MongoAppEntity : MongoEntity, IAppEntity
    {
        private LanguagesConfig languagesConfig;

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

        [BsonRequired]
        [BsonElement]
        public string MasterLanguage { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<MongoAppEntityLanguage> Languages { get; set; } = new List<MongoAppEntityLanguage>();

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppEntityClient> Clients { get; set; } = new Dictionary<string, MongoAppEntityClient>();

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppEntityContributor> Contributors { get; set; } = new Dictionary<string, MongoAppEntityContributor>();

        public PartitionResolver PartitionResolver
        {
            get { return LanguagesConfig.ToResolver(); }
        }

        public LanguagesConfig LanguagesConfig
        {
            get { return languagesConfig ?? (languagesConfig = CreateLanguagesConfig()); }
        }

        IReadOnlyCollection<IAppClientEntity> IAppEntity.Clients
        {
            get { return Clients.Values; }
        }

        IReadOnlyCollection<IAppContributorEntity> IAppEntity.Contributors
        {
            get { return Contributors.Values; }
        }

        public void UpdateLanguages(Func<LanguagesConfig, LanguagesConfig> updater)
        {
            var newConfig = updater(LanguagesConfig);

            if (languagesConfig != newConfig)
            {
                languagesConfig = newConfig;
                Languages = newConfig.OfType<LanguageConfig>().Select(FromLanguageConfig).ToList();

                MasterLanguage = newConfig.Master.Language;
            }
        }

        private LanguagesConfig CreateLanguagesConfig()
        {
            languagesConfig = LanguagesConfig.Create(Languages.Select(ToLanguageConfig).ToList());

            if (MasterLanguage != null)
            {
                languagesConfig = languagesConfig.MakeMaster(MasterLanguage);
            }

            return languagesConfig;
        }

        private static MongoAppEntityLanguage FromLanguageConfig(LanguageConfig l)
        {
            return new MongoAppEntityLanguage { Iso2Code = l.Language, IsOptional = l.IsOptional, Fallback = l.Fallback.Select(x => x.Iso2Code).ToList() };
        }

        private static LanguageConfig ToLanguageConfig(MongoAppEntityLanguage l)
        {
            return new LanguageConfig(l.Iso2Code, l.IsOptional, l.Fallback?.Select<string, Language>(f => f));
        }
    }
}
