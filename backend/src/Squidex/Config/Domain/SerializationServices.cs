// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using GraphQL.Server;
using Microsoft.Extensions.DependencyInjection;
using Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Domain.Apps.Entities.Contents.GraphQL;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Reflection;

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
                new ContentFieldDataConverter(),
                new DomainIdConverter(),
                new EnvelopeHeadersConverter(),
                new ExecutionResultJsonConverter(new ErrorInfoProvider()),
                new FilterConverter(),
                new InstantConverter(),
                new JsonValueConverter(),
                new LanguageConverter(),
                new LanguagesConfigConverter(),
                new NamedDomainIdConverter(),
                new NamedGuidIdConverter(),
                new NamedLongIdConverter(),
                new NamedStringIdConverter(),
                new PropertyPathConverter(),
                new RefTokenConverter(),
                new RoleConverter(),
                new RolesConverter(),
                new RuleConverter(),
                new SchemaConverter(),
                new StatusConverter(),
                new StringEnumConverter(),
                new WorkflowsConverter(),
                new WorkflowStepConverter(),
                new WriteonlyGeoJsonConverter());

            settings.NullValueHandling = NullValueHandling.Ignore;

            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateParseHandling = DateParseHandling.None;

            settings.TypeNameHandling = typeNameHandling;

            return settings;
        }

        public static IServiceCollection AddSquidexSerializers(this IServiceCollection services)
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

        public static IMvcBuilder AddSquidexSerializers(this IMvcBuilder builder)
        {
            builder.AddNewtonsoftJson(options =>
            {
                options.AllowInputFormatterExceptionMessages = false;

                ConfigureJson(options.SerializerSettings, TypeNameHandling.None);
            });

            return builder;
        }

        public static IGraphQLBuilder AddSquidexWriter(this IGraphQLBuilder builder)
        {
            builder.Services.AddSingleton<IDocumentWriter>(c =>
            {
                var settings = ConfigureJson(new JsonSerializerSettings(), TypeNameHandling.None);
                var serializer = new NewtonsoftJsonSerializer(settings);

                return new DefaultDocumentWriter(serializer);
            });

            return builder;
        }
    }
}
