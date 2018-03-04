// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.Apps.Services.Implementations;
using Squidex.Domain.Users;
using Squidex.Infrastructure;

namespace Squidex.Config.Domain
{
    public static class SubscriptionServices
    {
        public static void AddMySubscriptionServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs(c => c.GetService<IOptions<MyUsageOptions>>()?.Value?.Plans.OrEmpty());

            services.AddSingletonAs<ConfigAppPlansProvider>()
                .As<IAppPlansProvider>();

            services.AddSingletonAs<NoopAppPlanBillingManager>()
                .As<IAppPlanBillingManager>();

            services.AddSingletonAs<AssetUserPictureStore>()
                .As<IUserPictureStore>();

            services.AddSingletonAs<NoopUserEvents>()
                .As<IUserEvents>();
        }
    }
}
