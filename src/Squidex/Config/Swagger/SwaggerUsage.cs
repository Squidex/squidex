// ==========================================================================
//  SwaggerUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using NSwag.AspNetCore;
using Squidex.Config.Identity;
using Squidex.Infrastructure;

namespace Squidex.Config.Swagger
{
    public static class SwaggerUsage
    {
        public static void UseMySwagger(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyUrlsOptions>>().Value;

            var settings =
                new SwaggerOwinSettings { Title = "Squidex API Specification", IsAspNetCore = false}
                    .ConfigurePaths()
                    .ConfigureSchemaSettings()
                    .ConfigureIdentity(options);

            app.UseSwagger(typeof(SwaggerUsage).GetTypeInfo().Assembly, settings);
        }

        private static SwaggerOwinSettings ConfigurePaths(this SwaggerOwinSettings settings)
        {
            settings.SwaggerRoute = $"{Constants.ApiPrefix}/swagger/v1/swagger.json";

            settings.PostProcess = document =>
            {
                document.BasePath = Constants.ApiPrefix;
            };

            settings.MiddlewareBasePath = Constants.ApiPrefix;

            return settings;
        }

        private static SwaggerOwinSettings ConfigureSchemaSettings(this SwaggerOwinSettings settings)
        {
            settings.DefaultEnumHandling = EnumHandling.String;
            settings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;

            settings.TypeMappers = new List<ITypeMapper>
            {
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
