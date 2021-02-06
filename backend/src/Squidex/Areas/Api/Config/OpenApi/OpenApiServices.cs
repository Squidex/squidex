// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
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
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

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

            services.AddSingleton(c =>
            {
                var settings = ConfigureSchemaSettings(new JsonSchemaGeneratorSettings
                {
                    FlattenInheritanceHierarchy = true,
                    SerializerOptions = null,
                    SerializerSettings = c.GetRequiredService<JsonSerializerSettings>()
                });

                return new JsonSchemaGenerator(settings);
            });

            services.AddOpenApiDocument(settings =>
            {
                settings.ConfigureName();
                settings.ConfigureSchemaSettings();

                settings.OperationProcessors.Add(new ODataQueryParamsProcessor("/apps/{app}/assets", "assets", false));
            });

            services.AddTransient<SchemasOpenApiGenerator>();
        }

        private static void ConfigureName<T>(this T settings) where T : OpenApiDocumentGeneratorSettings
        {
            settings.Title = "Squidex API";
        }

        public static T ConfigureSchemaSettings<T>(this T settings) where T : JsonSchemaGeneratorSettings
        {
            settings.AllowReferencesWithProperties = true;

            settings.TypeMappers = new List<ITypeMapper>
            {
                CreateStringMap<DomainId>(),
                CreateStringMap<Instant>(JsonFormatStrings.DateTime),
                CreateStringMap<Language>(),
                CreateStringMap<NamedId<DomainId>>(),
                CreateStringMap<NamedId<Guid>>(),
                CreateStringMap<NamedId<string>>(),
                CreateStringMap<RefToken>(),
                CreateStringMap<Status>(),

                CreateObjectMap<JsonObject>(),
                CreateObjectMap<AssetMetadata>()
            };

            return settings;
        }

        private static ITypeMapper CreateObjectMap<T>()
        {
            return new PrimitiveTypeMapper(typeof(T), schema =>
            {
                schema.Type = JsonObjectType.Object;

                schema.AdditionalPropertiesSchema = new JsonSchema
                {
                    Description = "Any JSON type"
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
    }
}
