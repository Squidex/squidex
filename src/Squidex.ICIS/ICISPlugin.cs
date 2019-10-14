// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Squidex.ICIS.Authentication;
using Squidex.ICIS.Deployment;
using Squidex.ICIS.Kafka;
using Squidex.ICIS.UI;
using Squidex.ICIS.Validation;
using Squidex.Infrastructure.Plugins;

namespace Squidex.ICIS
{
    public sealed class ICISPlugin : IPlugin, IWebPlugin
    {
        public void ConfigureAfter(IApplicationBuilder app)
        {
        }

        public void ConfigureBefore(IApplicationBuilder app)
        {
            if (Debugger.IsAttached)
            {
                IdentityModelEventSource.ShowPII = true;
            }
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddAuthenticationServices(config);
            services.AddKafkaServices(config);
            services.AddValidationServices();
            services.AddDeployment(config);
            services.AddUI();
        }
    }
}