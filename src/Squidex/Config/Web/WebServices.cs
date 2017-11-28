// ==========================================================================
//  WebDependencies.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Config.Domain;
using Squidex.Pipeline;

namespace Squidex.Config.Web
{
    public static class WebServices
    {
        public static void AddMyMvc(this IServiceCollection services)
        {
            services.AddSingletonAs<FileCallbackResultExecutor>();

            services.AddSingletonAs<AppApiFilter>();
            services.AddSingletonAs<ApiCostsFilter>();

            services.AddMvc().AddMySerializers();
            services.AddCors();
            services.AddRouting();
        }
    }
}
