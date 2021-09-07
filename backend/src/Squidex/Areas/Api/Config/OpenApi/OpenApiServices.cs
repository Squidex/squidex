// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;
using NodaTime;
using NSwag.Generation;
using NSwag.Generation.Processors;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public static class OpenApiServices
    {
        public static void AddSquidexOpenApiSettings(this IServiceCollection services)
        {
            services.AddSingletonAs<ErrorDtoProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<RuleActionProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<CommonProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<XmlTagProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<SecurityProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<ScopesProcessor>()
                .As<IOperationProcessor>();

            services.AddSingletonAs<FixProcessor>()
                .As<IOperationProcessor>();

            services.AddSingletonAs<TagByGroupNameProcessor>()
                .As<IOperationProcessor>();

            services.AddSingletonAs<XmlResponseTypesProcessor>()
                .As<IOperationProcessor>();

            services.AddSingletonAs<JsonSchemaGenerator>()
                .AsSelf();

            services.AddSingletonAs<OpenApiSchemaGenerator>()
                .AsSelf();

            services.AddSingleton(c =>
            {
                var settings = new JsonSchemaGeneratorSettings
                {
                    SerializerSettings = c.GetRequiredService<JsonSerializerSettings>()
                };

                ConfigureSchemaSettings(settings, true);

                return settings;
            });

            services.AddSingleton(c =>
            {
                var settings = new OpenApiDocumentGeneratorSettings
                {
                    SerializerSettings = c.GetRequiredService<JsonSerializerSettings>()
                };

                ConfigureSchemaSettings(settings, true);

                foreach (var processor in c.GetRequiredService<IEnumerable<IDocumentProcessor>>())
                {
                    settings.DocumentProcessors.Add(processor);
                }

                return settings;
            });

            services.AddOpenApiDocument(settings =>
            {
                settings.Title = "Squidex API";

                ConfigureSchemaSettings(settings);

                settings.OperationProcessors.Add(new QueryParamsProcessor("/apps/{app}/assets"));
            });
        }

        private static void ConfigureSchemaSettings(JsonSchemaGeneratorSettings settings, bool flatten = false)
        {
            settings.AllowReferencesWithProperties = true;

            settings.ReflectionService = new ReflectionServices();

            settings.TypeMappers = new List<ITypeMapper>
            {
                CreateStringMap<DomainId>(),
                CreateStringMap<Instant>(JsonFormatStrings.DateTime),
                CreateStringMap<LocalDate>(JsonFormatStrings.Date),
                CreateStringMap<LocalDateTime>(JsonFormatStrings.DateTime),
                CreateStringMap<Language>(),
                CreateStringMap<NamedId<DomainId>>(),
                CreateStringMap<NamedId<Guid>>(),
                CreateStringMap<NamedId<string>>(),
                CreateStringMap<RefToken>(),
                CreateStringMap<Status>(),

                CreateObjectMap<JsonObject>(),
                CreateObjectMap<AssetMetadata>(),

                CreateAnyMap<IJsonValue>(),
                CreateAnyMap<FilterNode<IJsonValue>>()
            };

            settings.SchemaType = SchemaType.OpenApi3;

            settings.FlattenInheritanceHierarchy = flatten;
        }

        private static ITypeMapper CreateObjectMap<T>()
        {
            return new PrimitiveTypeMapper(typeof(T), schema =>
            {
                schema.Type = JsonObjectType.Object;

                schema.AdditionalPropertiesSchema = new JsonSchema
                {
                    Description = "Any"
                };
            });
        }

        private static ITypeMapper CreateStringMap<T>(string? format = null)
        {
            return new PrimitiveTypeMapper(typeof(T), schema =>
            {
                schema.Type = JsonObjectType.String;

                schema.Format = format;
            });
        }

        private static ITypeMapper CreateAnyMap<T>()
        {
            return new PrimitiveTypeMapper(typeof(T), schema =>
            {
                schema.Type = JsonObjectType.None;
            });
        }
    }
}
