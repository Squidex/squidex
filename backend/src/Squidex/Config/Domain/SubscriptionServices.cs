// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Config.Domain
{
    public static class SubscriptionServices
    {
        public static void AddSquidexSubscriptions(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs(c => c.GetRequiredService<IOptions<UsageOptions>>()?.Value?.Plans.OrEmpty()!);

            services.AddSingletonAs<ConfigAppPlansProvider>()
                .AsOptional<IAppPlansProvider>();

            services.AddSingletonAs<NoopAppPlanBillingManager>()
                .AsOptional<IAppPlanBillingManager>();

            services.AddSingletonAs<UsageGate>()
                .AsSelf();
        }
    }
}
