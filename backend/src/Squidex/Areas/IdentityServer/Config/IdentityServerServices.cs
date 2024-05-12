// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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

        services.AddSingletonAs<OpenIdConnectPostConfigureOptions>()
            .AsSelf();

        services.AddSingletonAs<DynamicSchemeProvider>()
            .AsSelf().As<IAuthenticationSchemeProvider>().As<IOptionsMonitor<DynamicOpenIdConnectOptions>>();

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
                    builder.UseSingletonHandler<AlwaysAddScopeHandler>()
                        .SetOrder(AttachSignInParameters.Descriptor.Order + 1);
                });

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

                builder.SetAccessTokenLifetime(TimeSpan.FromDays(30));
            })
            .AddValidation(builder =>
            {
                builder.UseLocalServer();
                builder.UseAspNetCore();

                builder.Configure(options =>
                {
                    options.Issuer = options.Configuration?.Issuer;
                });
            });

        services.Configure<AntiforgeryOptions>((c, options) =>
        {
            var identityOptions = c.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

            options.SuppressXFrameOptionsHeader = identityOptions.SuppressXFrameOptionsHeader;

            // Set antiforgery cookie secure policy to always for https
            var baseUrl = c.GetRequiredService<IUrlGenerator>().BuildUrl();

            if (baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            }
        });

        services.Configure<OpenIddictServerOptions>((c, options) =>
        {
            var urlGenerator = c.GetRequiredService<IUrlGenerator>();

            var identityPrefix = Constants.PrefixIdentityServer;
            var identityOptions = c.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

            Uri BuildUrl(string path)
            {
                return new Uri($"{identityPrefix.TrimStart('/')}/{path}", UriKind.Relative);
            }

            options.Issuer = new Uri(urlGenerator.BuildUrl());

            options.AuthorizationEndpointUris.SetEndpoint(
                BuildUrl("connect/authorize"));

            options.IntrospectionEndpointUris.SetEndpoint(
                BuildUrl("connect/introspect"));

            options.LogoutEndpointUris.SetEndpoint(
                BuildUrl("connect/logout"));

            options.TokenEndpointUris.SetEndpoint(
                BuildUrl("connect/token"));

            options.UserinfoEndpointUris.SetEndpoint(
                BuildUrl("connect/userinfo"));

            options.CryptographyEndpointUris.SetEndpoint(
                BuildUrl(".well-known/jwks"));

            options.ConfigurationEndpointUris.SetEndpoint(
                BuildUrl(".well-known/openid-configuration"));
        });
    }

    private static void SetEndpoint(this List<Uri> endpointUris, Uri uri)
    {
        endpointUris.Clear();
        endpointUris.Add(uri);
    }
}
