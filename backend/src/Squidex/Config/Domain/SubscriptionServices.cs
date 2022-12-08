// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Config.Domain;

public static class SubscriptionServices
{
    public static void AddSquidexSubscriptions(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingletonAs(c => c.GetRequiredService<IOptions<UsageOptions>>()?.Value?.Plans.OrEmpty()!);

        services.AddSingletonAs<ConfigPlansProvider>()
            .AsOptional<IBillingPlans>();

        services.AddSingletonAs<NoopBillingManager>()
            .AsOptional<IBillingManager>();

        services.AddSingletonAs<UsageGate>()
            .AsOptional<IUsageGate>().As<IAssetUsageTracker>();
    }
}
