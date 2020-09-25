// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
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

            services.AddOpenApiDocument(settings =>
            {
                settings.ConfigureName();
                settings.ConfigureSchemaSettings();

                settings.OperationProcessors.Add(new ODataQueryParamsProcessor("/apps/{app}/assets", "assets", false));
            });

            services.AddTransient<SchemasOpenApiGenerator>();
        }

        public static void ConfigureName<T>(this T settings) where T : OpenApiDocumentGeneratorSettings
        {
            settings.Title = "Squidex API";
        }

        public static void ConfigureSchemaSettings<T>(this T settings) where T : OpenApiDocumentGeneratorSettings
        {
            settings.TypeMappers = new List<ITypeMapper>
            {
                CreateStringMap<Instant>(JsonFormatStrings.DateTime),
                CreateStringMap<Language>(),
                CreateStringMap<DomainId>(),
                CreateStringMap<RefToken>(),
                CreateStringMap<Status>(),

                CreateObjectMap<JsonObject>(),
                CreateObjectMap<AssetMetadata>()
            };
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
