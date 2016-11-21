// ==========================================================================
//  LazyClientStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Services;

namespace Squidex.Configurations.Identity
{
    public class LazyClientStore : IClientStore
    {
        private readonly IAppProvider appProvider;
        private readonly Dictionary<string, Client> staticClients = new Dictionary<string, Client>(StringComparer.OrdinalIgnoreCase);

        public LazyClientStore(IOptions<MyIdentityOptions> identityOptions, IAppProvider appProvider)
        {
            Guard.NotNull(identityOptions, nameof(identityOptions));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.appProvider = appProvider;

            CreateStaticClients(identityOptions);
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            var client = staticClients.GetOrDefault(clientId);

            if (client == null)
            {
                return null;
            }

            var app = await appProvider.FindAppByNameAsync(clientId);

            if (app != null)
            {
                client = CreateClientFromApp(app);
            }

            return client;
        }

        private void CreateStaticClients(IOptions<MyIdentityOptions> identityOptions)
        {
            foreach (var client in CreateStaticClients(identityOptions.Value))
            {
                staticClients[client.ClientId] = client;
            }
        }

        private static Client CreateClientFromApp(IAppEntity app)
        {
            var id = app.Name;

            return new Client
            {
                ClientId = id,
                ClientName = id,
                ClientSecrets = app.ClientKeys.Select(x => new Secret(x.ClientKey, x.ExpiresUtc)).ToList(),
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string>
                {
                    Constants.ApiScope
                }
            };
        }

        private static IEnumerable<Client> CreateStaticClients(MyIdentityOptions options)
        {
            const string id = Constants.FrontendClient;

            yield return new Client
            {
                ClientId = id,
                ClientName = id,
                RedirectUris = new List<string>
                {
                    options.BuildUrl("login;"),
                    options.BuildUrl("identity-server/client-callback-silent/"),
                    options.BuildUrl("identity-server/client-callback-popup/")
                },
                PostLogoutRedirectUris = new List<string>
                {
                    options.BuildUrl("logout", false)
                },
                AllowAccessTokensViaBrowser = true,
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = new List<string>
                {
                    StandardScopes.OpenId.Name,
                    StandardScopes.Profile.Name,
                    Constants.ApiScope,
                    Constants.ProfileScope
                },
                RequireConsent = false
            };
        }
    }
}
