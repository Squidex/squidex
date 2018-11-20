// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using NodaTime;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Security;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Config;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;

namespace Squidex.Areas.Api.Config.Swagger
{
    public static class SwaggerServices
    {
        public static void AddMySwaggerSettings(this IServiceCollection services)
        {
            services.AddSingletonAs<RuleActionProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<XmlTagProcessor>()
                .As<IDocumentProcessor>();

            services.AddSingletonAs<TagByGroupNameProcessor>()
                .As<IOperationProcessor>();

            services.AddSingletonAs<XmlResponseTypesProcessor>()
                .As<IOperationProcessor>();

            services.AddSingleton(c =>
            {
                var settings = new SwaggerDocumentSettings { SchemaType = SchemaType.OpenApi3 };

                return new SwaggerDocumentRegistration(settings.DocumentName, generator);
            }))

                

            settings.DocumentProcessors.Add(new RuleActionProcessor());
            settings.DocumentProcessors.Add(new XmlTagProcessor());

            settings.OperationProcessors.Add(new TagByGroupNameProcessor());
            settings.OperationProcessors.Add(new XmlResponseTypesProcessor());

            services.AddOpenApiDocument(configure =>
            {
                var urlOptions = configure.GetService<IOptions<MyUrlsOptions>>().Value;

                configure.AddAssetODataParams();
                configure.ConfigureNames();
                configure.ConfigurePaths(urlOptions);
                configure.ConfigureSchemaSettings();
                configure.ConfigureIdentity(urlOptions);
            });

            services.AddTransient<SchemasSwaggerGenerator>();
        }

        public static void AddAssetODataParams<T>(this T settings) where T : SwaggerGeneratorSettings
        {
            settings.OperationProcessors.Add(new ODataQueryParamsProcessor("/apps/{app}/assets", "assets", false));
        }

        public static void ConfigureNames<T>(this T settings) where T : SwaggerGeneratorSettings
        {
            settings.Title = "Squidex API";
        }

        public static void ConfigureIdentity<T>(this T settings, MyUrlsOptions urlOptions) where T : SwaggerGeneratorSettings
        {
            settings.DocumentProcessors.Add(
                new SecurityDefinitionAppender(
                    Constants.SecurityDefinition, SwaggerHelper.CreateOAuthSchema(urlOptions)));

            settings.OperationProcessors.Add(new ScopesProcessor());
        }

        public static void ConfigureSchemaSettings<T>(this T settings) where T : SwaggerGeneratorSettings
        {
            settings.DefaultEnumHandling = EnumHandling.String;
            settings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;

            settings.TypeMappers = new List<ITypeMapper>
            {
                new PrimitiveTypeMapper(typeof(Instant), schema =>
                {
                    schema.Type = JsonObjectType.String;
                    schema.Format = JsonFormatStrings.DateTime;
                }),
                new PrimitiveTypeMapper(typeof(Language), s => s.Type = JsonObjectType.String),
                new PrimitiveTypeMapper(typeof(RefToken), s => s.Type = JsonObjectType.String)
            };

            settings.DocumentProcessors.Add(new RuleActionProcessor());
            settings.DocumentProcessors.Add(new XmlTagProcessor());

            settings.OperationProcessors.Add(new TagByGroupNameProcessor());
            settings.OperationProcessors.Add(new XmlResponseTypesProcessor());
        }
    }
}
