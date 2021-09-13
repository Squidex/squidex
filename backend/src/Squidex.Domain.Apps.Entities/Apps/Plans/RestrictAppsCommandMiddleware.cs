// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class RestrictAppsCommandMiddleware : ICommandMiddleware
    {
        private readonly RestrictAppsOptions usageOptions;
        private readonly IUserResolver userResolver;

        public RestrictAppsCommandMiddleware(IOptions<RestrictAppsOptions> usageOptions, IUserResolver userResolver)
        {
            this.usageOptions = usageOptions.Value;
            this.userResolver = userResolver;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            if (usageOptions.MaximumNumberOfApps <= 0 || context.Command is not CreateApp createApp || createApp.Actor.IsClient)
            {
                await next(context);
                return;
            }

            var totalApps = 0;

            var user = await userResolver.FindByIdAsync(createApp.Actor.Identifier);

            if (user != null)
            {
                totalApps = user.Claims.GetTotalApps();

                if (totalApps >= usageOptions.MaximumNumberOfApps)
                {
                    throw new ValidationException(T.Get("apps.maximumTotalReached"));
                }
            }

            await next(context);

            if (context.IsCompleted && user != null)
            {
                var newApps = totalApps + 1;
                var newAppsValue = newApps.ToString(CultureInfo.InvariantCulture);

                await userResolver.SetClaimAsync(user.Id, SquidexClaimTypes.TotalApps, newAppsValue, true);
            }
        }
    }
}
