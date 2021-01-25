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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Users;
using Squidex.Hosting;
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
        private readonly Dictionary<string, Client> staticClients = new Dictionary<string, Client>(StringComparer.OrdinalIgnoreCase);

        public LazyClientStore(IServiceProvider serviceProvider)
        {
            Guard.NotNull(serviceProvider, nameof(serviceProvider));

            this.serviceProvider = serviceProvider;

            CreateStaticClients();
        }

        public async Task<Client?> FindClientByIdAsync(string clientId)
        {
            var client = staticClients.GetOrDefault(clientId);

            if (client != null)
            {
                return client;
            }

            var (appName, appClientId) = clientId.GetClientParts();

            var appProvider = serviceProvider.GetRequiredService<IAppProvider>();

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
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var user = await userService.FindByIdAsync(clientId);

                if (user == null)
                {
                    return null;
                }

                var secret = user.Claims.ClientSecret();

                if (!string.IsNullOrWhiteSpace(secret))
                {
                    return CreateClientFromUser(user, secret);
                }
            }

            return null;
        }

        private static Client CreateClientFromUser(IUser user, string secret)
        {
            return new Client
            {
                ClientId = user.Id,
                ClientName = $"{user.Email} Client",
                ClientClaimsPrefix = null,
                ClientSecrets = new List<Secret>
                {
                    new Secret(secret.Sha256())
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

        private void CreateStaticClients()
        {
            var identityOptions = serviceProvider.GetRequiredService<IOptions<MyIdentityOptions>>().Value;

            var urlGenerator = serviceProvider.GetRequiredService<IUrlGenerator>();

            foreach (var client in CreateStaticClients(urlGenerator, identityOptions))
            {
                staticClients[client.ClientId] = client;
            }
        }

        private static IEnumerable<Client> CreateStaticClients(IUrlGenerator urlGenerator, MyIdentityOptions identityOptions)
        {
            var frontendId = Constants.FrontendClient;

            yield return new Client
            {
                ClientId = frontendId,
                ClientName = frontendId,
                RedirectUris = new List<string>
                {
                    urlGenerator.BuildUrl("login;"),
                    urlGenerator.BuildUrl("client-callback-silent", false),
                    urlGenerator.BuildUrl("client-callback-popup", false)
                },
                PostLogoutRedirectUris = new List<string>
                {
                    urlGenerator.BuildUrl("logout", false)
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
                    urlGenerator.BuildUrl($"{Constants.PortalPrefix}/signin-internal", false),
                    urlGenerator.BuildUrl($"{Constants.OrleansPrefix}/signin-internal", false)
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
