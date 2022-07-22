// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Migrations;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Apps.Json;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Contents.Json;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Json.System;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Config.Domain
{
    public static class SerializationServices
    {
        private static JsonSerializerOptions ConfigureJson(TypeNameRegistry typeNameRegistry, JsonSerializerOptions? options = null)
        {
            options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            options.Converters.Add(new InheritanceConverter<IEvent>(typeNameRegistry));
            options.Converters.Add(new InheritanceConverter<FieldProperties>(typeNameRegistry));
            options.Converters.Add(new InheritanceConverter<RuleAction>(typeNameRegistry));
            options.Converters.Add(new InheritanceConverter<RuleTrigger>(typeNameRegistry));
            options.Converters.Add(new JsonValueConverter());
            options.Converters.Add(new ReadonlyDictionaryConverterFactory());
            options.Converters.Add(new ReadonlyListConverterFactory());
            options.Converters.Add(new SurrogateJsonConverter<ClaimsPrincipal, ClaimsPrincipalSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<FilterNode<JsonValue>, JsonFilterSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<LanguageConfig, LanguageConfigSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<LanguagesConfig, LanguagesConfigSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<Roles, RolesSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<Rule, RuleSorrgate>());
            options.Converters.Add(new SurrogateJsonConverter<Schema, SchemaSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<WorkflowStep, WorkflowStepSurrogate>());
            options.Converters.Add(new SurrogateJsonConverter<WorkflowTransition, WorkflowTransitionSurrogate>());
            options.Converters.Add(new StringConverter<CompareOperator>());
            options.Converters.Add(new StringConverter<DomainId>());
            options.Converters.Add(new StringConverter<NamedId<DomainId>>());
            options.Converters.Add(new StringConverter<NamedId<Guid>>());
            options.Converters.Add(new StringConverter<NamedId<string>>());
            options.Converters.Add(new StringConverter<Language>());
            options.Converters.Add(new StringConverter<PropertyPath>(x => x));
            options.Converters.Add(new StringConverter<RefToken>());
            options.Converters.Add(new StringConverter<Status>());
            options.Converters.Add(new JsonStringEnumConverter());
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.IncludeFields = true;

            return options;
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

            services.AddSingletonAs<FieldTypeProvider>()
                .As<ITypeProvider>();

            services.AddSingletonAs<SystemJsonSerializer>()
                .As<IJsonSerializer>();

            services.AddSingletonAs<TypeNameRegistry>()
                .AsSelf();

            services.AddSingletonAs(c => ConfigureJson(c.GetRequiredService<TypeNameRegistry>()))
                .As<JsonSerializerOptions>();

            return services;
        }

        public static IMvcBuilder AddSquidexSerializers(this IMvcBuilder builder)
        {
            builder.Services.Configure<JsonOptions>((c, options) =>
            {
                ConfigureJson(c.GetRequiredService<TypeNameRegistry>(), options.JsonSerializerOptions);
            });

            return builder;
        }
    }
}
