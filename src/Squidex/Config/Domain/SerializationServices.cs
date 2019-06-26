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
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Config.Domain
{
    public static class SerializationServices
    {
        private static JsonSerializerSettings ConfigureJson(JsonSerializerSettings settings, TypeNameHandling typeNameHandling)
        {
            settings.Converters.Add(new StringEnumConverter());

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
                new StatusConverter(),
                new StringEnumConverter(),
                new WorkflowConverter());

            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.TypeNameHandling = typeNameHandling;

            return settings;
        }

        public static IServiceCollection AddMySerializers(this IServiceCollection services)
        {
            services.AddSingletonAs<AutoAssembyTypeProvider<SquidexCoreModel>>()
                .As<ITypeProvider>();

            services.AddSingletonAs<AutoAssembyTypeProvider<SquidexEvents>>()
                .As<ITypeProvider>();

            services.AddSingletonAs<AutoAssembyTypeProvider<SquidexInfrastructure>>()
                .As<ITypeProvider>();

            services.AddSingletonAs<AutoAssembyTypeProvider<SquidexMigrations>>()
                .As<ITypeProvider>();

            services.AddSingletonAs<FieldRegistry>()
                .As<ITypeProvider>();

            services.AddSingletonAs<NewtonsoftJsonSerializer>()
                .As<IJsonSerializer>();

            services.AddSingletonAs<SerializationInitializer>()
                .AsSelf();

            services.AddSingletonAs<TypeNameRegistry>()
                .AsSelf();

            services.AddSingletonAs(c => JsonSerializer.Create(c.GetRequiredService<JsonSerializerSettings>()))
                .AsSelf();

            services.AddSingletonAs(c =>
                {
                    var serializerSettings = ConfigureJson(new JsonSerializerSettings(), TypeNameHandling.Auto);

                    var typeNameRegistry = c.GetService<TypeNameRegistry>();

                    if (typeNameRegistry != null)
                    {
                        serializerSettings.SerializationBinder = new TypeNameSerializationBinder(typeNameRegistry);
                    }

                    return serializerSettings;
                }).As<JsonSerializerSettings>();

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
