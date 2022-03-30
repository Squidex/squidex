// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Squidex.Domain.Users;
using Squidex.Domain.Users.InMemory;
using Squidex.Hosting;
using Squidex.Web;
using Squidex.Web.Pipeline;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;
using static OpenIddict.Server.OpenIddictServerHandlers;

namespace Squidex.Areas.IdentityServer.Config
{
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

                    var identityServer = Constants.PrefixIdentityServer;

                    builder.SetAuthorizationEndpointUris($"{identityServer}/connect/authorize");
                    builder.SetIntrospectionEndpointUris($"{identityServer}/connect/introspect");
                    builder.SetLogoutEndpointUris($"{identityServer}/connect/logout");
                    builder.SetTokenEndpointUris($"{identityServer}/connect/token");
                    builder.SetUserinfoEndpointUris($"{identityServer}/connect/userinfo");
                    builder.SetCryptographyEndpointUris($"{identityServer}/.well-known/jwks");
                    builder.SetConfigurationEndpointUris($"{identityServer}/.well-known/openid-configuration");
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

            services.Configure<OpenIddictServerOptions>((services, options) =>
            {
                var urlGenerator = services.GetRequiredService<IUrlGenerator>();

                var issuerUrl = Constants.PrefixIdentityServer;

                /*
                options.AuthorizationEndpointUris.Add(
                     new Uri($"{issuerUrl}/connect/authorize", UriKind.Relative));

                options.IntrospectionEndpointUris.Add(
                     new Uri($"{issuerUrl}/connect/introspect", UriKind.Relative));

                options.LogoutEndpointUris.Add(
                     new Uri($"{issuerUrl}/connect/logout", UriKind.Relative));

                options.TokenEndpointUris.Add(
                     new Uri($"{issuerUrl}/connect/token", UriKind.Relative));

                options.UserinfoEndpointUris.Add(
                     new Uri($"{issuerUrl}/connect/userinfo", UriKind.Relative));

                options.CryptographyEndpointUris.Add(
                     new Uri($"{issuerUrl}/.well-known/jwks", UriKind.Relative));

                options.ConfigurationEndpointUris.Add(
                     new Uri($"{issuerUrl}/.well-known/openid-configuration", UriKind.Relative));*/

                options.Issuer = new Uri(urlGenerator.BuildUrl());
            });
        }
    }
}
