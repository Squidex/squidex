// ==========================================================================
//  GoogleIdentityUsage.cs
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
    public static class GoogleIdentityUsage
    {
        public static IApplicationBuilder UseMyGoogleAuthentication(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

            if (options.IsGoogleAuthConfigured())
            {
                var googleOptions =
                    new GoogleOptions
                    {
                        ClientId = options.GoogleClient,
                        ClientSecret = options.GoogleSecret,
                        Events = new GoogleHandler()
                    };

                app.UseGoogleAuthentication(googleOptions);
            }

            return app;
        }
    }
}
