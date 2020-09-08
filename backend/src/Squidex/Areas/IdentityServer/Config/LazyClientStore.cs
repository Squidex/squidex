// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Config
{
    public class LazyClientStore : IClientStore
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAppProvider appProvider;
        private readonly Dictionary<string, Client> staticClients = new Dictionary<string, Client>(StringComparer.OrdinalIgnoreCase);

        public LazyClientStore(
            IServiceProvider serviceProvider,
            IOptions<UrlsOptions> urlsOptions,
            IOptions<MyIdentityOptions> identityOptions,
            IAppProvider appProvider)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(identityOptions, nameof(identityOptions));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(urlsOptions, nameof(urlsOptions));

            this.serviceProvider = serviceProvider;

            this.appProvider = appProvider;

            CreateStaticClients(urlsOptions, identityOptions);
        }

        public async Task<Client?> FindClientByIdAsync(string clientId)
        {
            var client = staticClients.GetOrDefault(clientId);

            if (client != null)
            {
                return client;
            }

            var (appName, appClientId) = clientId.GetClientParts();

            if (!string.IsNullOrWhiteSpace(appName) && !string.IsNullOrWhiteSpace(appClientId))
            {
                var app = await appProvider.GetAppAsync(appName, true);

                var appClient = app?.Clients.GetOrDefault(appClientId);

                if (appClient != null)
                {
                    return CreateClientFromApp(clientId, appClient);
                }
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var user = await userManager.FindByIdWithClaimsAsync(clientId);

                if (!string.IsNullOrWhiteSpace(user?.ClientSecret()))
                {
                    return CreateClientFromUser(user);
                }
            }

            return null;
        }

        private static Client CreateClientFromUser(UserWithClaims user)
        {
            return new Client
            {
                ClientId = user.Id,
                ClientName = $"{user.Email} Client",
                ClientClaimsPrefix = null,
                ClientSecrets = new List<Secret>
                {
                    new Secret(user.ClientSecret().Sha256())
                },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string>
                {
                    Constants.ApiScope,
                    Constants.RoleScope,
                    Constants.PermissionsScope
                },
                Claims = new List<ClientClaim>
                {
                    new ClientClaim(OpenIdClaims.Subject, user.Id)
                }
            };
        }

        private static Client CreateClientFromApp(string id, AppClient appClient)
        {
            return new Client
            {
                ClientId = id,
                ClientName = id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(appClient.Secret.Sha256())
                },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string>
                {
                    Constants.ApiScope,
                    Constants.RoleScope,
                    Constants.PermissionsScope
                }
            };
        }

        private void CreateStaticClients(IOptions<UrlsOptions> urlsOptions, IOptions<MyIdentityOptions> identityOptions)
        {
            foreach (var client in CreateStaticClients(urlsOptions.Value, identityOptions.Value))
            {
                staticClients[client.ClientId] = client;
            }
        }

        private static IEnumerable<Client> CreateStaticClients(UrlsOptions urlsOptions, MyIdentityOptions identityOptions)
        {
            var frontendId = Constants.FrontendClient;

            yield return new Client
            {
                ClientId = frontendId,
                ClientName = frontendId,
                RedirectUris = new List<string>
                {
                    urlsOptions.BuildUrl("login;"),
                    urlsOptions.BuildUrl("client-callback-silent", false),
                    urlsOptions.BuildUrl("client-callback-popup", false)
                },
                PostLogoutRedirectUris = new List<string>
                {
                    urlsOptions.BuildUrl("logout", false)
                },
                AllowAccessTokensViaBrowser = true,
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    Constants.ApiScope,
                    Constants.PermissionsScope,
                    Constants.ProfileScope,
                    Constants.RoleScope
                },
                RequireConsent = false
            };

            var internalClient = Constants.InternalClientId;

            yield return new Client
            {
                ClientId = internalClient,
                ClientName = internalClient,
                ClientSecrets = new List<Secret>
                {
                    new Secret(Constants.InternalClientSecret)
                },
                RedirectUris = new List<string>
                {
                    urlsOptions.BuildUrl($"{Constants.PortalPrefix}/signin-internal", false),
                    urlsOptions.BuildUrl($"{Constants.OrleansPrefix}/signin-internal", false)
                },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    Constants.ApiScope,
                    Constants.PermissionsScope,
                    Constants.ProfileScope,
                    Constants.RoleScope
                },
                RequireConsent = false
            };

            if (identityOptions.IsAdminClientConfigured())
            {
                var id = identityOptions.AdminClientId;

                yield return new Client
                {
                    ClientId = id,
                    ClientName = id,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(identityOptions.AdminClientSecret.Sha256())
                    },
                    AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = new List<string>
                    {
                        Constants.ApiScope,
                        Constants.RoleScope,
                        Constants.PermissionsScope
                    },
                    Claims = new List<ClientClaim>
                    {
                        new ClientClaim(SquidexClaimTypes.Permissions, Permissions.All)
                    }
                };
            }
        }
    }
}
