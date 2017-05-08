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
using Squidex.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Apps;

// ReSharper disable InvertIf
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Squidex.Read.MongoDb.Apps
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

        [BsonRequired]
        [BsonElement]
        public string MasterLanguage { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<MongoAppLanguage> Languages { get; set; } = new List<MongoAppLanguage>(); 

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppClientEntity> Clients { get; set; } = new Dictionary<string, MongoAppClientEntity>();

        [BsonRequired]
        [BsonElement]
        public Dictionary<string, MongoAppContributorEntity> Contributors { get; set; } = new Dictionary<string, MongoAppContributorEntity>();

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
                Languages = newConfig.Select(FromLanguageConfig).ToList();

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

        private static MongoAppLanguage FromLanguageConfig(LanguageConfig l)
        {
            return new MongoAppLanguage { Iso2Code = l.Language, IsOptional = l.IsOptional, Fallback = l.Fallback.Select(x => x.Iso2Code).ToList() };
        }

        private static LanguageConfig ToLanguageConfig(MongoAppLanguage l)
        {
            return new LanguageConfig(l.Iso2Code, l.IsOptional, l.Fallback?.Select<string, Language>(f => f));
        }
    }
}
