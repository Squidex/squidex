// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Squidex.Hosting.Web;

namespace Squidex.Config.Authentication;

public static class AuthenticationServices
{
    public static void AddSquidexAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var identityOptions = config.GetSection("identity").Get<MyIdentityOptions>() ?? new ();

        services.AddAuthentication()
            .AddSquidexCookies(config)
            .AddSquidexExternalGithubAuthentication(identityOptions)
            .AddSquidexExternalGoogleAuthentication(identityOptions)
            .AddSquidexExternalMicrosoftAuthentication(identityOptions)
            .AddSquidexExternalOdic(identityOptions)
            .AddSquidexIdentityServerAuthentication(identityOptions, config);
    }

    public static AuthenticationBuilder AddSquidexCookies(this AuthenticationBuilder builder, IConfiguration config)
    {
        var urlsOptions = config.GetSection("urls").Get<UrlOptions>() ?? new ();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.AccessDeniedPath = "/identity-server/account/access-denied";
            options.LoginPath = "/identity-server/account/login";
            options.LogoutPath = "/identity-server/account/login";

            options.Cookie.Name = ".sq.auth2";

            if (urlsOptions.BaseUrl?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == true)
            {
                options.Cookie.SameSite = SameSiteMode.None;
            }
        });

        return builder.AddCookie();
    }
}
