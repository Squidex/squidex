// ==========================================================================
//  ICIS Copyright
// ==========================================================================
//  Copyright (c) ICIS
//  All rights reserved.
// ==========================================================================

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Squidex.ICIS.Authentication;
using Squidex.ICIS.Kafka;
using Squidex.ICIS.Validation;

namespace Squidex.ICIS.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddIcisServices(this IServiceCollection services, IConfiguration config)
        {
            if (Debugger.IsAttached)
            {
                IdentityModelEventSource.ShowPII = true;
            }

            AuthenticationServiceExtensions.AddAuthenticationServices(services, config);
            KafkaServiceExtensions.AddKafkaServices(services, config);
            ValidationServiceExtensions.AddValidationServices(services);
        }
    }
}