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
using NSwag.SwaggerGeneration;
using NSwag.SwaggerGeneration.Processors;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Config.Swagger
{
    public static class SwaggerServices
    {
        public static void AddMySwaggerSettings(this IServiceCollection services)
        {
            services.AddSingletonAs<ErrorDtoProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<RuleActionProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<ThemeProcessor>()
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

            services.AddTransient<SchemasSwaggerGenerator>();
        }

        public static void ConfigureName<T>(this T settings) where T : SwaggerGeneratorSettings
        {
            settings.Title = "Squidex API";
        }

        public static void ConfigureSchemaSettings<T>(this T settings) where T : SwaggerGeneratorSettings
        {
            settings.TypeMappers = new List<ITypeMapper>
            {
                new PrimitiveTypeMapper(typeof(Instant), schema =>
                {
                    schema.Type = JsonObjectType.String;
                    schema.Format = JsonFormatStrings.DateTime;
                }),

                new PrimitiveTypeMapper(typeof(Language), s => s.Type = JsonObjectType.String),
                new PrimitiveTypeMapper(typeof(RefToken), s => s.Type = JsonObjectType.String),
                new PrimitiveTypeMapper(typeof(Status), s => s.Type = JsonObjectType.String),
            };
        }
    }
}
