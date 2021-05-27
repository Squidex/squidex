// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Entities.Apps.Templates;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Extensions.Samples.Middleware
{
    public sealed class TemplatePlugin : IPlugin
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<ICustomCommandMiddleware, TemplateMiddleware>();

            services.AddSingleton<ITemplate, TemplateInstance>();
        }
    }
}
