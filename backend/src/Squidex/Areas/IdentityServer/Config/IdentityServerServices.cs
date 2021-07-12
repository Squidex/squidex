// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Squidex.Domain.Users;
using Squidex.Domain.Users.InMemory;
using Squidex.Hosting;
using Squidex.Web;
using Squidex.Web.Pipeline;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreHandlers;
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
                    builder.RemoveEventHandler(ValidateTransportSecurityRequirement.Descriptor);

                    builder.AddEventHandler<ProcessSignInContext>(builder =>
                    {
                        builder.UseSingletonHandler<AlwaysAddTokenHandler>()
                            .SetOrder(AttachTokenParameters.Descriptor.Order + 1);
                    });

                    builder
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo");

                    builder.DisableAccessTokenEncryption();

                    builder.RegisterScopes(
                        Scopes.Email,
                        Scopes.Profile,
                        Scopes.Roles,
                        Constants.ScopeApi,
                        Constants.ScopePermissions);

                    builder.SetAccessTokenLifetime(TimeSpan.FromDays(30));

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

                options.Issuer = new Uri(urlGenerator.BuildUrl("/identity-server", false));
            });
        }
    }
}
