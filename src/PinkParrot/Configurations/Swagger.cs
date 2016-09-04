// ==========================================================================
//  Swagger.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using PinkParrot.Pipeline.Swagger;
using Swashbuckle.Swagger.Model;

namespace PinkParrot.Configurations
{
    public static class Swagger
    {
        public static void AddAppSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info { Title = "Pink Parrot", Version = "v1" });
                options.OperationFilter<HidePropertyFilter>();
                options.OperationFilter<CamelCaseParameterFilter>();
                options.SchemaFilter<HidePropertyFilter>();
                options.SchemaFilter<RemoveReadonlyFilter>();
                options.IncludeXmlComments(GetXmlCommentsPath(PlatformServices.Default.Application));
            });
        }

        public static void UseAppSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUi();
        }

        private static string GetXmlCommentsPath(ApplicationEnvironment appEnvironment)
        {
            return Path.Combine(appEnvironment.ApplicationBasePath, "PinkParrot.xml");
        }
    }
}
