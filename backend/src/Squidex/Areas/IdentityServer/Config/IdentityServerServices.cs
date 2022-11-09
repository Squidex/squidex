// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Squidex.Config;
using Squidex.Domain.Users;
using Squidex.Domain.Users.InMemory;
using Squidex.Hosting;
using Squidex.Web;
using Squidex.Web.Pipeline;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;
using static OpenIddict.Server.OpenIddictServerHandlers;

namespace Squidex.Areas.IdentityServer.Config;

public static class IdentityServerServices
{
    public static void AddSquidexIdentityServer(this IServiceCollection services)
    {
        services.Configure<KeyManagementOptions>((c, options) =>
        {
            options.XmlRepository = c.GetRequiredService<IXmlRepository>();
        });

        services.AddDataProtection()
            .SetApplicationName("Squidex");

        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddDefaultTokenProviders();

        services.AddSingletonAs<DefaultXmlRepository>()
            .As<IXmlRepository>();

        services.AddScopedAs<DefaultUserService>()
            .As<IUserService>();

        services.AddScopedAs<UserClaimsPrincipalFactoryWithEmail>()
            .As<IUserClaimsPrincipalFactory<IdentityUser>>();

        services.AddSingletonAs<ApiPermissionUnifier>()
            .As<IClaimsTransformation>();

        services.AddSingletonAs<TokenStoreInitializer>()
            .AsSelf();

        services.AddSingletonAs<CreateAdminInitializer>()
            .AsSelf();

        services.ConfigureOptions<DefaultKeyStore>();

        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
        });

        services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder.Services.AddSingletonAs<IdentityServerConfiguration.Scopes>()
                    .As<IOpenIddictScopeStore<ImmutableScope>>();

                builder.Services.AddSingletonAs<DynamicApplicationStore>()
                    .As<IOpenIddictApplicationStore<ImmutableApplication>>();

                builder.ReplaceApplicationManager(typeof(ApplicationManager<>));
            })
            .AddServer(builder =>
            {
                builder.AddEventHandler<ProcessSignInContext>(builder =>
                {
                    builder.UseSingletonHandler<AlwaysAddTokenHandler>()
                        .SetOrder(AttachTokenParameters.Descriptor.Order + 1);
                });

                builder.SetAccessTokenLifetime(TimeSpan.FromDays(30));

                builder.DisableAccessTokenEncryption();

                builder.RegisterScopes(
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.Roles,
                    Constants.ScopeApi,
                    Constants.ScopePermissions);

                builder.AllowClientCredentialsFlow();
                builder.AllowImplicitFlow();
                builder.AllowAuthorizationCodeFlow();

                builder.UseAspNetCore()
                    .DisableTransportSecurityRequirement()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.Configure<AntiforgeryOptions>((c, options) =>
        {
            var identityOptions = c.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

            options.SuppressXFrameOptionsHeader = identityOptions.SuppressXFrameOptionsHeader;
        });

        services.Configure<OpenIddictServerOptions>((c, options) =>
        {
            var urlGenerator = c.GetRequiredService<IUrlGenerator>();

            var identityPrefix = Constants.PrefixIdentityServer;
            var identityOptions = c.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

            Func<string, Uri> buildUrl;

            if (identityOptions.MultipleDomains)
            {
                buildUrl = url => new Uri($"{identityPrefix}{url}", UriKind.Relative);

                options.Issuer = new Uri(urlGenerator.BuildUrl());
            }
            else
            {
                buildUrl = url => new Uri(urlGenerator.BuildUrl($"{identityPrefix}{url}", false));

                options.Issuer = new Uri(urlGenerator.BuildUrl(identityPrefix, false));
            }

            options.AuthorizationEndpointUris.SetEndpoint(
                buildUrl("/connect/authorize"));

            options.IntrospectionEndpointUris.SetEndpoint(
                buildUrl("/connect/introspect"));

            options.LogoutEndpointUris.SetEndpoint(
                buildUrl("/connect/logout"));

            options.TokenEndpointUris.SetEndpoint(
                buildUrl("/connect/token"));

            options.UserinfoEndpointUris.SetEndpoint(
                buildUrl("/connect/userinfo"));

            options.CryptographyEndpointUris.SetEndpoint(
                buildUrl("/.well-known/jwks"));

            options.ConfigurationEndpointUris.SetEndpoint(
                buildUrl("/.well-known/openid-configuration"));
        });
    }

    private static void SetEndpoint(this List<Uri> endpointUris, Uri uri)
    {
        endpointUris.Clear();
        endpointUris.Add(uri);
    }
}
