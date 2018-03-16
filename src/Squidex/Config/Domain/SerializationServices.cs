// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Migrate_01;
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
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Config.Domain
{
    public static class SerializationServices
    {
        private static readonly TypeNameRegistry TypeNameRegistry =
             new TypeNameRegistry()
                 .MapUnmapped(SquidexCoreModel.Assembly)
                 .MapUnmapped(SquidexEvents.Assembly)
                 .MapUnmapped(SquidexInfrastructure.Assembly)
                 .MapUnmapped(SquidexMigrations.Assembly);
        private static readonly FieldRegistry FieldRegistry = new FieldRegistry(TypeNameRegistry);

        public static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings();
        public static readonly JsonSerializer DefaultJsonSerializer;

        private static void ConfigureJson(JsonSerializerSettings settings, TypeNameHandling typeNameHandling)
        {
            settings.SerializationBinder = new TypeNameSerializationBinder(TypeNameRegistry);

            settings.ContractResolver = new ConverterContractResolver(
                new AppClientsConverter(),
                new AppContributorsConverter(),
                new AppPatternsConverter(),
                new ClaimsPrincipalConverter(),
                new InstantConverter(),
                new LanguageConverter(),
                new LanguagesConfigConverter(),
                new NamedGuidIdConverter(),
                new NamedLongIdConverter(),
                new NamedStringIdConverter(),
                new PropertiesBagConverter<EnvelopeHeaders>(),
                new PropertiesBagConverter<PropertiesBag>(),
                new RefTokenConverter(),
                new RuleConverter(),
                new SchemaConverter(FieldRegistry),
                new StringEnumConverter());

            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.TypeNameHandling = typeNameHandling;

            settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        static SerializationServices()
        {
            ConfigureJson(DefaultJsonSettings, TypeNameHandling.Auto);

            DefaultJsonSerializer = JsonSerializer.Create(DefaultJsonSettings);

            BsonJsonConvention.Register(DefaultJsonSerializer);
        }

        public static IServiceCollection AddMySerializers(this IServiceCollection services)
        {
            services.AddSingletonAs(t => FieldRegistry);
            services.AddSingletonAs(t => DefaultJsonSettings);
            services.AddSingletonAs(t => DefaultJsonSerializer);
            services.AddSingletonAs(t => TypeNameRegistry);

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
