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
            services.AddSingleton(typeof(SwaggerSettings), s =>
            {
                var urlOptions = s.GetService<IOptions<MyUrlsOptions>>().Value;

                var settings =
                    new SwaggerSettings { Title = "Squidex API", Version = "1.0", IsAspNetCore = false }
                        .AddAssetODataParams()
                        .ConfigurePaths(urlOptions)
                        .ConfigureSchemaSettings()
                        .ConfigureIdentity(urlOptions);

                return settings;
            });

            services.AddTransient<SchemasSwaggerGenerator>();
        }

        private static SwaggerSettings AddAssetODataParams(this SwaggerSettings settings)
        {
            settings.OperationProcessors.Add(new ODataQueryParamsProcessor("/apps/{app}/assets", "assets", false));

            return settings;
        }

        private static SwaggerSettings ConfigureIdentity(this SwaggerSettings settings, MyUrlsOptions urlOptions)
        {
            settings.DocumentProcessors.Add(
                new SecurityDefinitionAppender(
                    Constants.SecurityDefinition, SwaggerHelper.CreateOAuthSchema(urlOptions)));

            settings.OperationProcessors.Add(new ScopesProcessor());

            return settings;
        }

        private static SwaggerSettings ConfigurePaths(this SwaggerSettings settings, MyUrlsOptions urlOptions)
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

        private static SwaggerSettings ConfigureSchemaSettings(this SwaggerSettings settings)
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

            settings.DocumentProcessors.Add(new XmlTagProcessor());

            settings.OperationProcessors.Add(new XmlTagProcessor());
            settings.OperationProcessors.Add(new XmlResponseTypesProcessor());

            return settings;
        }
    }
}
