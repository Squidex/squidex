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
using NSwag.SwaggerGeneration.Processors.Security;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Config;
using Squidex.Infrastructure;
using Squidex.Pipeline.Swagger;

namespace Squidex.Areas.Api.Config.Swagger
{
    public static class SwaggerServices
    {
        public static void AddMySwaggerSettings(this IServiceCollection services)
        {
            services.AddSingleton(typeof(SwaggerSettings<SwaggerGeneratorSettings>), s =>
            {
                var urlOptions = s.GetService<IOptions<MyUrlsOptions>>().Value;

                var settings = new SwaggerSettings<SwaggerGeneratorSettings>()
                        .AddAssetODataParams()
                        .ConfigureNames()
                        .ConfigurePaths(urlOptions)
                        .ConfigureSchemaSettings()
                        .ConfigureIdentity(urlOptions);

                return settings;
            });

            services.AddTransient<SchemasSwaggerGenerator>();
        }

        public static SwaggerSettings<T> ConfigureNames<T>(this SwaggerSettings<T> settings) where T : SwaggerGeneratorSettings, new()
        {
            settings.GeneratorSettings.Title = "Squidex API";
            settings.GeneratorSettings.Version = "1.0";

            return settings;
        }

        public static SwaggerSettings<T> AddAssetODataParams<T>(this SwaggerSettings<T> settings) where T : SwaggerGeneratorSettings, new()
        {
            settings.GeneratorSettings.OperationProcessors.Add(new ODataQueryParamsProcessor("/apps/{app}/assets", "assets", false));

            return settings;
        }

        public static SwaggerSettings<T> ConfigureIdentity<T>(this SwaggerSettings<T> settings, MyUrlsOptions urlOptions) where T : SwaggerGeneratorSettings, new()
        {
            settings.GeneratorSettings.DocumentProcessors.Add(
                new SecurityDefinitionAppender(
                    Constants.SecurityDefinition, SwaggerHelper.CreateOAuthSchema(urlOptions)));

            settings.GeneratorSettings.OperationProcessors.Add(new ScopesProcessor());

            return settings;
        }

        public static SwaggerSettings<T> ConfigurePaths<T>(this SwaggerSettings<T> settings, MyUrlsOptions urlOptions) where T : SwaggerGeneratorSettings, new()
        {
            settings.SwaggerRoute = $"{Constants.ApiPrefix}/swagger/v1/swagger.json";

            settings.PostProcess = document =>
            {
                document.BasePath = Constants.ApiPrefix;
                document.Info.ExtensionData = new Dictionary<string, object>
                {
                    ["x-logo"] = new { url = urlOptions.BuildUrl("images/logo-white.png", false), backgroundColor = "#3f83df" }
                };
            };

            settings.MiddlewareBasePath = Constants.ApiPrefix;

            return settings;
        }

        public static SwaggerSettings<T> ConfigureSchemaSettings<T>(this SwaggerSettings<T> settings) where T : SwaggerGeneratorSettings, new()
        {
            settings.GeneratorSettings.DefaultEnumHandling = EnumHandling.String;
            settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;

            settings.GeneratorSettings.TypeMappers = new List<ITypeMapper>
            {
                new PrimitiveTypeMapper(typeof(Instant), schema =>
                {
                    schema.Type = JsonObjectType.String;
                    schema.Format = JsonFormatStrings.DateTime;
                }),
                new PrimitiveTypeMapper(typeof(Language), s => s.Type = JsonObjectType.String),
                new PrimitiveTypeMapper(typeof(RefToken), s => s.Type = JsonObjectType.String)
            };

            settings.GeneratorSettings.DocumentProcessors.Add(new XmlTagProcessor());

            settings.GeneratorSettings.OperationProcessors.Add(new XmlTagProcessor());
            settings.GeneratorSettings.OperationProcessors.Add(new XmlResponseTypesProcessor());

            return settings;
        }
    }
}
