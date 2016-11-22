// ==========================================================================
//  SwaggerUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using NSwag;
using NSwag.AspNetCore;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors.Contexts;
using Squidex.Configurations.Identity;
using Squidex.Infrastructure;
using Squidex.Modules.Api;
using Squidex.Pipeline;

namespace Squidex.Configurations.Web
{
    public static class SwaggerUsage
    {
        public sealed class DescriptionResponseTypeAttributeProcessor : IOperationProcessor
        {
            private readonly WebApiToSwaggerGeneratorSettings settings;
            
            public DescriptionResponseTypeAttributeProcessor(WebApiToSwaggerGeneratorSettings settings)
            {
                this.settings = settings;
            }

            public bool Process(OperationProcessorContext context)
            {
                context.OperationDescription.Operation.Responses.Remove("200");

                var responseTypes =
                    context.MethodInfo.GetCustomAttributes<DescribedResponseTypeAttribute>().ToList();

                responseTypes.Add(new DescribedResponseTypeAttribute(500, typeof(ErrorDto), "Operation failed."));

                foreach (var attribute in responseTypes)
                {
                    var responseType = attribute.Type;
             
                    var typeDescription = 
                        JsonObjectTypeDescription.FromType(responseType, 
                            context.MethodInfo.ReturnParameter?.GetCustomAttributes(), settings.DefaultEnumHandling);

                    var responseCode = attribute.StatusCode.ToString(CultureInfo.InvariantCulture);
                    var response = new SwaggerResponse { Description = attribute.Description };

                    if (IsVoidResponse(responseType) == false)
                    {
                        response.IsNullableRaw = typeDescription.IsNullable;
                        response.Schema = context.SwaggerGenerator.GenerateAndAppendSchemaFromType(responseType, typeDescription.IsNullable, null);
                    }

                    context.OperationDescription.Operation.Responses[responseCode] = response;

                }

                return true;
            }

            private static bool IsVoidResponse(Type returnType)
            {
                return returnType == null || returnType == typeof(void);
            }
        }

        public static void UseMySwagger(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyUrlsOptions>>().Value;

            var settings =
                new SwaggerOwinSettings { Title = "Squidex API Specification" }
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
                new PrimitiveTypeMapper(typeof(Language), s => s.Type = JsonObjectType.String)
            };

            settings.OperationProcessors.Add(new DescriptionResponseTypeAttributeProcessor(settings));

            return settings;
        }
    }
}
