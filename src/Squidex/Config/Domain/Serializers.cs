// ==========================================================================
//  Serializers.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Config.Domain
{
    public static class Serializers
    {
        private static readonly TypeNameRegistry TypeNameRegistry = new TypeNameRegistry();
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();
        private static readonly FieldRegistry FieldRegistry = new FieldRegistry(TypeNameRegistry);

        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings, TypeNameHandling typeNameHandling)
        {
            settings.SerializationBinder = new TypeNameSerializationBinder(TypeNameRegistry);

            settings.ContractResolver = new ConverterContractResolver(
                new AppClientsConverter(),
                new AppContributorsConverter(),
                new ClaimsPrincipalConverter(),
                new InstantConverter(),
                new LanguageConverter(),
                new LanguagesConfigConverter(),
                new NamedGuidIdConverter(),
                new NamedLongIdConverter(),
                new NamedStringIdConverter(),
                new PropertiesBagConverter(),
                new RefTokenConverter(),
                new RuleConverter(),
                new SchemaConverter(FieldRegistry),
                new StringEnumConverter());

            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.TypeNameHandling = typeNameHandling;

            settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            return settings;
        }

        static Serializers()
        {
            TypeNameRegistry.MapUnmapped(typeof(SquidexCoreModel).Assembly);
            TypeNameRegistry.MapUnmapped(typeof(SquidexEvents).Assembly);
            TypeNameRegistry.MapUnmapped(typeof(SquidexInfrastructure).Assembly);

            ConfigureJson(SerializerSettings, TypeNameHandling.Auto);

            BsonJsonConvention.Register(JsonSerializer.Create(SerializerSettings));
        }

        public static IServiceCollection AddMyEventFormatter(this IServiceCollection services)
        {
            services.AddSingleton(t => TypeNameRegistry);
            services.AddSingleton(t => FieldRegistry);
            services.AddSingleton(t => SerializerSettings);
            services.AddSingleton(t => JsonSerializer.Create(SerializerSettings));

            return services;
        }

        public static IMvcBuilder AddMySerializers(this IMvcBuilder mvc)
        {
            mvc.AddJsonOptions(options =>
            {
                ConfigureJson(options.SerializerSettings, TypeNameHandling.None);
            });

            return mvc;
        }
    }
}
