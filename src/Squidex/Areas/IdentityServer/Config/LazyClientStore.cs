﻿// ==========================================================================
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
using Microsoft.Extensions.Options;
using Squidex.Config;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;

namespace Squidex.Areas.IdentityServer.Config
{
    public class LazyClientStore : IClientStore
    {
        private readonly IAppProvider appProvider;
        private readonly Dictionary<string, Client> staticClients = new Dictionary<string, Client>(StringComparer.OrdinalIgnoreCase);

        public LazyClientStore(IOptions<MyUrlsOptions> urlsOptions, IAppProvider appProvider)
        {
            Guard.NotNull(urlsOptions, nameof(urlsOptions));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;

            CreateStaticClients(urlsOptions);
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var client = staticClients.GetOrDefault(clientId);

            if (client != null)
            {
                return client;
            }

            var token = clientId.Split(':');

            if (token.Length != 2)
            {
                return null;
            }

            var app = await appProvider.GetAppAsync(token[0]);

            var appClient = app?.Clients.GetOrDefault(token[1]);

            if (appClient == null)
            {
                return null;
            }

            client = CreateClientFromApp(clientId, appClient);

            return client;
        }

        private static Client CreateClientFromApp(string id, AppClient appClient)
        {
            return new Client
            {
                ClientId = id,
                ClientName = id,
                ClientSecrets = new List<Secret> { new Secret(appClient.Secret.Sha256()) },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string>
                {
                    Constants.ApiScope,
                    Constants.RoleScope
                }
            };
        }

        private void CreateStaticClients(IOptions<MyUrlsOptions> urlsOptions)
        {
            foreach (var client in CreateStaticClients(urlsOptions.Value))
            {
                staticClients[client.ClientId] = client;
            }
        }

        private static IEnumerable<Client> CreateStaticClients(MyUrlsOptions urlsOptions)
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
                ClientSecrets = new List<Secret> { new Secret(Constants.InternalClientSecret) },
                RedirectUris = new List<string>
                {
                    urlsOptions.BuildUrl($"{Constants.PortalPrefix}/signin-oidc", false)
                },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    Constants.ApiScope,
                    Constants.ProfileScope,
                    Constants.RoleScope
                },
                RequireConsent = false
            };
        }
    }
}
