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
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Domain.Apps.Events;
using Squidex.Extensions.Actions;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Config.Domain
{
    public static class SerializationServices
    {
        private static readonly TypeNameRegistry TypeNameRegistry =
            new TypeNameRegistry()
                .MapFields()
                .MapRules()
                .MapRuleActions()
                .MapUnmapped(SquidexCoreModel.Assembly)
                .MapUnmapped(SquidexEvents.Assembly)
                .MapUnmapped(SquidexInfrastructure.Assembly)
                .MapUnmapped(SquidexMigrations.Assembly);

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
                new EnvelopeHeadersConverter(),
                new InstantConverter(),
                new JsonValueConverter(),
                new LanguageConverter(),
                new LanguagesConfigConverter(),
                new NamedGuidIdConverter(),
                new NamedLongIdConverter(),
                new NamedStringIdConverter(),
                new RefTokenConverter(),
                new RolesConverter(),
                new RuleConverter(),
                new SchemaConverter(),
                new StringEnumConverter());

            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.TypeNameHandling = typeNameHandling;
        }

        static SerializationServices()
        {
            ConfigureJson(DefaultJsonSettings, TypeNameHandling.Auto);

            DefaultJsonSerializer = JsonSerializer.Create(DefaultJsonSettings);
        }

        public static IServiceCollection AddMySerializers(this IServiceCollection services)
        {
            services.AddSingleton(DefaultJsonSettings);
            services.AddSingleton(DefaultJsonSerializer);
            services.AddSingleton(TypeNameRegistry);

            services.AddSingleton<IJsonSerializer>(new NewtonsoftJsonSerializer(DefaultJsonSettings));

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
