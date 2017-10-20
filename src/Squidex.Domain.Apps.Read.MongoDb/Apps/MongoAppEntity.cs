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

namespace Squidex.Domain.Apps.Read.MongoDb.Apps
{
    public sealed class MongoAppEntity : MongoEntity, IAppEntity
    {
        private readonly IReadOnlyDictionary<string, IAppClientEntity> clientWrapper;
        private readonly IReadOnlyDictionary<string, IAppContributorEntity> contributorWrapper;
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
        public List<string> ContributorIds { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<MongoAppEntityLanguage> Languages { get; set; }

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppEntityClient> Clients { get; set; }

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppEntityContributor> Contributors { get; set; }

        public PartitionResolver PartitionResolver
        {
            get { return LanguagesConfig.ToResolver(); }
        }

        public LanguagesConfig LanguagesConfig
        {
            get { return languagesConfig ?? (languagesConfig = CreateLanguagesConfig()); }
        }

        IReadOnlyDictionary<string, IAppClientEntity> IAppEntity.Clients
        {
            get { return clientWrapper; }
        }

        IReadOnlyDictionary<string, IAppContributorEntity> IAppEntity.Contributors
        {
            get { return contributorWrapper; }
        }

        public MongoAppEntity()
        {
            clientWrapper = new DictionaryWrapper<string, IAppClientEntity, MongoAppEntityClient>(() => Clients);

            contributorWrapper = new DictionaryWrapper<string, IAppContributorEntity, MongoAppEntityContributor>(() => Contributors);
        }

        public void ChangePlan(string planId, RefToken planOwner)
        {
            PlanId = planId;

            PlanOwner = planOwner.Identifier;
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
            languagesConfig = LanguagesConfig.Create(Languages?.Select(ToLanguageConfig).ToList() ?? new List<LanguageConfig>());

            if (MasterLanguage != null)
            {
                languagesConfig = languagesConfig.MakeMaster(MasterLanguage);
            }

            return languagesConfig;
        }

        private static MongoAppEntityLanguage FromLanguageConfig(LanguageConfig l)
        {
            return new MongoAppEntityLanguage { Iso2Code = l.Language, IsOptional = l.IsOptional, Fallback = l.LanguageFallbacks.Select(x => x.Iso2Code).ToList() };
        }

        private static LanguageConfig ToLanguageConfig(MongoAppEntityLanguage l)
        {
            return new LanguageConfig(l.Iso2Code, l.IsOptional, l.Fallback?.Select<string, Language>(f => f));
        }
    }
}
