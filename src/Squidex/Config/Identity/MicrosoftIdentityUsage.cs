// ==========================================================================
//  MicrosoftIdentityUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Squidex.Config.Identity
{
    public static class MicrosoftIdentityUsage
    {
        public static IApplicationBuilder UseMyMicrosoftAuthentication(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

            if (options.IsMicrosoftAuthConfigured())
            {
                var googleOptions =
                    new MicrosoftAccountOptions
                    {
                        ClientId = options.MicrosoftClient,
                        ClientSecret = options.MicrosoftSecret,
                        Events = new MicrosoftHandler()
                    };

                app.UseMicrosoftAccountAuthentication(googleOptions);
            }

            return app;
        }
    }
}
